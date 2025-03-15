using Microsoft.EntityFrameworkCore;
using SnapchatLib;
using SnapchatLib.Exceptions;
using SnapProto.Snapchat.Ads.Render.Schema;
using SnapProto.Snapchat.Janus.Api;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.WorkTask;

public class CreateAccountTask : BaseWorkTask
{
    private readonly EmailManager _emailManager;
    private readonly SnapchatActionRunner _runner;
    private readonly Utilities _utilities;
    private readonly AppSettingsLoader _settingsLoader;
    private readonly FakePersonGenerator _fakePersonGenerator;
    private readonly WorkScheduler _scheduler;
    private readonly IProxyManager _proxyManager;
    private readonly SnapchatAccountManager _accountManager;

    private string MaxGeneratedAccountsErrorMessage(AppSettings settings) =>
        $"You have reached the maximum amount of generated accounts ({settings.MaxCreatedAccounts}) for the day. Please wait until tomorrow to create more accounts";

    public CreateAccountTask(WorkScheduler scheduler, WorkLogger logger, EmailManager emailManager,
        FakePersonGenerator fakePersonGenerator, SnapchatActionRunner runner, Utilities utilities,
        AppSettingsLoader settingsLoader, IServiceProvider serviceProvider, IProxyManager proxyManager, SnapchatAccountManager accountManager) : base(logger, serviceProvider)
    {
        _emailManager = emailManager;
        _runner = runner;
        _utilities = utilities;
        _settingsLoader = settingsLoader;
        _fakePersonGenerator = fakePersonGenerator;
        _scheduler = scheduler;
        _proxyManager = proxyManager;
        _accountManager = accountManager;
    }

    public static string Extract(string input, int len)
    {
        if (string.IsNullOrEmpty(input) || input.Length < len)
        {
            return input;
        };

        return input.Substring(0, len);
    }
    
    private OS GetSelectedOs(AccountOSSelection osSelection)
    {
        // Choose android or IOS depending on UI settings
        return osSelection switch
        {
            AccountOSSelection.Android => OS.android,
            //AccountOSSelection.iOS => OS.ios,
            AccountOSSelection.Random => ChooseRandomOs(),
            _ => OS.android
        };
    }

    private OS ChooseRandomOs()
    {
        var values = Enum.GetValues(typeof(OS));
        return (OS)values.GetValue(_utilities.RandomNext(values.Length));
    }

    private async Task<string> GetSuggestedUsername(FakePerson fakePerson, OS os, ProxyGroup? proxyGroup, CancellationToken token)
    {
        var suggested = await _runner.SuggestUsername(os, fakePerson.FirstName, fakePerson.LastName, proxyGroup, token);

        return suggested.SuggestionsArray.FirstOrDefault();
    }

    private async Task<ValidationStatus> VerifyPhone(PhoneVerificationService service, string countryIso,
        WorkLogger taskLogger, WorkRequest work, SnapchatAccountModel account, ProxyGroup proxyGroup)
    {
        IPhoneVerificator? verificator = null;
        switch (service)
        {
            case PhoneVerificationService.FiveSim:
                await taskLogger.LogDebug(work, "Created FiveSim verification service", account);
                verificator = FiveSimVerificator.FromServiceProvider(ServiceProvider);
                break;
            case PhoneVerificationService.SMSActivate:
                await taskLogger.LogDebug(work, "Created SmsActivate verification service", account);
                verificator = SmsActivateActivator.FromServiceProvider(ServiceProvider);
                break;
            case PhoneVerificationService.SmsPool:
                await taskLogger.LogDebug(work, "Created SmsPool verification service", account);
                verificator = SmsPoolActivator.FromServiceProvider(ServiceProvider);
                break;
            case PhoneVerificationService.TextVerified:
                await taskLogger.LogDebug(work, "Created TextVerified verification service", account);
                verificator = TextVerifiedActivator.FromServiceProvider(ServiceProvider);
                break;
        }

        try
        {
            return verificator == null
                ? ValidationStatus.NotValidated
                : await verificator.TryVerification(account, Country.GetCountry(countryIso),
                    proxyGroup, work.CancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            await taskLogger.LogError(work, $"Phone validation failed: {e.Message} {e.StackTrace}", account);
            return ValidationStatus.FailedValidation;
        }
    }

    private async Task<ValidationStatus> VerifyEmail(WorkLogger taskLogger, WorkRequest work, SnapchatAccountModel account, EmailModel email, CancellationToken token = default)
    {
        IEmailValidator validator;
        switch (email.Domain)
        {
            case "hotmail.com":
                await taskLogger.LogDebug(work, "Created Hotmail verification service", account);
                validator = OutlookValidator.FromServiceProvider(ServiceProvider);
                break;
            case "outlook.com":
                await taskLogger.LogDebug(work, "Created Outlook verification service", account);
                validator = OutlookValidator.FromServiceProvider(ServiceProvider);
                break;
            case "yahoo.com":
                await taskLogger.LogDebug(work, "Created Yahoo verification service", account);
                validator = YahooValidator.FromServiceProvider(ServiceProvider);
                break;
            case "gmail.com":
                await taskLogger.LogDebug(work, "Created Gmail verification service", account);
                validator = GmailValidator.FromServiceProvider(ServiceProvider);
                break;
            case "gmx.com":
                await taskLogger.LogDebug(work, "Created Gmx verification service", account);
                validator = GmxValidator.FromServiceProvider(ServiceProvider);
                break;
            case "yandex.ru":
                await taskLogger.LogDebug(work, "Created Yandex verification service", account);
                validator = YandexValidator.FromServiceProvider(ServiceProvider);
                break;
            default:
                return ValidationStatus.NotValidated;
        }

        return await validator.Validate(account, email, token);
    }

    private async Task<bool> HasReachedMaximumCreationLimit(AppSettings settings)
    {
        // To prevent spam, we also want to allow a max of 10x MaxManagedAccounts per day
        using var scope = ServiceProvider.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var creationTasks = await context.WorkRequests.Where(w => w.Action == WorkAction.CreateAccounts && w.RequestTime.Date == DateTime.UtcNow.Date).SumAsync(w => w.AccountsPass);
        
        return creationTasks >= settings.MaxCreatedAccounts;
    }

    // This method ALWAYS needs to have a scope of its own, otherwise context conflicts might happen
    private async Task<WorkStatus> DoWork(WorkRequest work, CreateAccountArguments arguments, AppSettings settings)
    {
        if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

        if (arguments.PhoneVerificationService == PhoneVerificationService.None && arguments.EmailVerificationService == EmailVerificationService.None)
        {
            await Logger.LogError(work, "You need to select at least one verification service");
            return WorkStatus.Error;
        }

        // We check here for an early exit without spooling up any other services and stuff
        if (await HasReachedMaximumCreationLimit(settings))
        {
            await Logger.LogError(work, MaxGeneratedAccountsErrorMessage(settings));
            return WorkStatus.Error;
        }
        
        // Services that might use a context instance need to be resolved here
        using var scope = ServiceProvider.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // We will try to generate one account a number of X times
        var attempts = 0;
        var created = false;
        
        var proxyGroup = await ProxyGroup.GetFromDatabase(arguments.ProxyGroup, ServiceProvider);

        while (attempts < settings.MaxRegisterAttempts)
        {
            if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;
            
            // And now we check here because we want to check before starting the creation process
            if (await HasReachedMaximumCreationLimit(settings))
            {
                await Logger.LogError(work, MaxGeneratedAccountsErrorMessage(settings));
                return WorkStatus.Error;
            }

            EmailModel? email = null;
            SnapchatAccountModel account = null;
            var koopechkaRequest = arguments.EmailVerificationService == EmailVerificationService.Kopeechka ? new KoopechkaSingleRequest(settings, _proxyManager) : null;
            try
            {
                var nameManager = scope.ServiceProvider.GetRequiredService<NameManager>();
                FakePerson fakePerson;

                if (arguments.NameCreationService == NameCreationService.Manager)
                {
                    var nameK = await nameManager.Take();

                    fakePerson = _fakePersonGenerator.Generate(email, arguments.Gender, nameK.FirstName,
                        nameK.LastName);
                }
                else
                {
                    fakePerson = _fakePersonGenerator.Generate(email, arguments.Gender, arguments.FirstName,
                        arguments.LastName);
                }

                // this is always null, why check for it?
                email ??= new EmailModel() { Address = fakePerson.Email, Password = null, IsFake = true };

                if (arguments.EmailVerificationService == EmailVerificationService.Provided)
                {
                    try
                    {
                        email = await _emailManager.GetAvailable(work.CancellationTokenSource.Token);
                        await Logger.LogDebug(work, $"Using address: {email.Address}");
                    }
                    catch (NoEmailAvailableException)
                    {
                        // As discussed with Justxn and Awhile in ticket-0135, users would expect that if this fails then the task should fail 
                        await Logger.LogError(work, "No available e-mails.");
                        return WorkStatus.Error;
                    }
                }
                else if (arguments.EmailVerificationService == EmailVerificationService.Kopeechka)
                {
                    // When using kopeechka, we still create a fake record but need to update the address
                    email.Address = koopechkaRequest?.Address;
                }

                var os = GetSelectedOs(arguments.OSSelection);

                if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

                var username = string.Empty;
                try
                {
                    var usernameManager = scope.ServiceProvider.GetRequiredService<UserNameManager>();

                    if (arguments.UserNameCreationService == UserNameCreationService.Manager &&
                        usernameManager._names.Any())
                    {
                        var GetName = await usernameManager.Take();

                        username = GetName.UserName.ToLower();
                    }
                    else
                    {
                        try
                        {
                            Random rand = new();
                            username = Faker.Internet.UserName($"{Extract(fakePerson.FirstName, 7)} {Extract(fakePerson.LastName, 4)}") + rand.Next(100, 10000);

                            if (username?.Length > 15) { username = username.Substring(0, 15); }
                            
                            //username = await GetSuggestedUsername(fakePerson, os, proxyGroup,
                            //    work.CancellationTokenSource.Token);
                        }
                        catch (NoAvailableProxyException)
                        {
                            await Logger.LogError(work, $"Register Failed. (Ran out of proxies)");
                            _scheduler.FailWorkAccount(work, null).Wait(work.CancellationTokenSource.Token);
                            return WorkStatus.Error;
                        }
                        catch (Exception ex)
                        {
                            
                            await Logger.LogError(work, $"Register Failed. (Failed to get a suggested username)");
                            Console.WriteLine($"Register Failed: {ex.Message}{ex.StackTrace}.");
                            _scheduler.FailWorkAccount(work, null).Wait(work.CancellationTokenSource.Token);
                            return WorkStatus.Error;
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    await Logger.LogDebug(work,
                        $"CreateAccount Job {work.Id}: Suggested username response came back as null. Retrying");
                    attempts++;
                    continue;
                }

                // Cancel before trying to register
                if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

                var passwordToUse =
                    !string.IsNullOrWhiteSpace(arguments.CustomPassword) && arguments.CustomPassword.Length > 0
                        ? arguments.CustomPassword
                        : fakePerson.Password;
                
                try
                {
                    account = await _runner.Register(username, passwordToUse, fakePerson.FirstName,
                        fakePerson.LastName, email.Address, email.Password, os, arguments.SnapchatVersion, proxyGroup,
                        work.CancellationTokenSource.Token);
                }
                catch (NoAvailableProxyException)
                {
                    await Logger.LogError(work, $"Register Failed. (Ran out of proxies)");
                    _scheduler.FailWorkAccount(work, null).Wait(work.CancellationTokenSource.Token);
                    return WorkStatus.Error;
                }
                catch (Exception ex)
                {
                    
                    await Logger.LogError(work, $"Register Failed. (Actually failed upon trying to register)");
                    Console.WriteLine($"Register Failed: {ex.Message}{ex.StackTrace}.");
                    _scheduler.FailWorkAccount(work, null).Wait(work.CancellationTokenSource.Token);
                    return WorkStatus.Error;
                }

                if (arguments.EmailVerificationService != EmailVerificationService.None)
                {
                    try
                    {
                        await _runner.ResendVerifyEmail(account, proxyGroup, work.CancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        await Logger.LogError(work,
                            $"Register Failed. (We couldn't request a verification email so we're going to consider this failed.)");
                        Console.WriteLine($"Register Failed: {ex.Message}{ex.StackTrace}.");
                        _scheduler.FailWorkAccount(work, null).Wait(work.CancellationTokenSource.Token);
                        return WorkStatus.Error;
                    }
                }
                
                if (account != null)
                {
                    if (context.Proxies != null && !context.Proxies.Local.Any(e => e.Id == account.Proxy.Id))
                        context.Attach(account.Proxy);

                    context.Add(account);
                    await context.SaveChangesAsync(work.CancellationTokenSource.Token);
                    await Logger.LogInformation(work,
                        $"Account created: {account.Username}:{account.Password} - {email.Address}:{email.Password} - {fakePerson.Name}");

                    // At this point, the account was already registered with the given email and the selected bitmoji
                    // we also need to make sure our email is in the database if it's a fake one (no validation service or koopechka)
                    if (email.IsFake)
                    {
                        await context.Emails.AddAsync(email, work.CancellationTokenSource.Token);
                        await context.SaveChangesAsync(work.CancellationTokenSource.Token);
                    }

                    await _emailManager.AssignEmail(account, email, context);

                    if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

                    /*if ((account.SnapClient.VerificationMethod == SCJanusVerificationStatus.Types
                            .SCJanusVerificationStatus_VerificationMethod.PhoneOnly || account.SnapClient.VerificationMethod == SCJanusVerificationStatus.Types
                            .SCJanusVerificationStatus_VerificationMethod.PhoneFirstEmailBypassed || account.SnapClient.VerificationMethod == SCJanusVerificationStatus.Types
                            .SCJanusVerificationStatus_VerificationMethod.PhoneFirstEmailSkippable) && arguments.PhoneVerificationService != PhoneVerificationService.None)
                    {
                        await _scheduler.FailWorkAccount(work, null).WaitAsync(work.CancellationTokenSource.Token);
                        return WorkStatus.Error;
                    }
                    else if(arguments.EmailVerificationService == EmailVerificationService.None)
                    {
                        await _scheduler.FailWorkAccount(work, null).WaitAsync(work.CancellationTokenSource.Token);
                        return WorkStatus.Error;
                    }*/

                    var phoneValidationTask = VerifyPhone(arguments.PhoneVerificationService, arguments.CountryISO,
                        Logger, work, account, proxyGroup);
                    var emailValidationTask = Task.FromResult(ValidationStatus.NotValidated);

                    switch (arguments.EmailVerificationService)
                    {
                        case EmailVerificationService.Provided:
                            emailValidationTask = VerifyEmail(Logger, work, account, email, work.CancellationTokenSource.Token);
                            break;
                        case EmailVerificationService.Kopeechka:
                            emailValidationTask = koopechkaRequest.WaitForValidationEmail(work, account);
                            break;
                        default:
                            break;
                    }
                    
                    Task.WaitAll(phoneValidationTask, emailValidationTask);

                    account.PhoneValidated = phoneValidationTask.Result;
                    
                    if (account.EmailValidated != ValidationStatus.Validated)
                    {
                        account.EmailValidated = emailValidationTask.Result;
                    }

                    if (work.CancellationTokenSource.IsCancellationRequested) return WorkStatus.Cancelled;

                    if (account.ValidationStatus == ValidationStatus.FailedValidation)
                    {
                        await Logger.LogError(work, "Validation was not completed successfully", account);
                        await _scheduler.UpdateWorkAddPass(work);
                    }
                    else
                    {
                        // Break out of the while so that we continue to our next account
                        if (arguments.BoostScore == 0)
                        {
                            await _scheduler.UpdateWorkAddPass(work);
                        }
                        else
                        {
                            await Logger.LogInformation(work, $"  Phone Validation: {account.PhoneValidated}");
                            await Logger.LogInformation(work, $"  E-Mail Validation: {account.EmailValidated}");

                            created = true;

                            if (arguments.BoostScore > 0)
                            {
                                await BoostScoreAction(_settingsLoader, _accountManager, _scheduler, _runner, arguments,
                                    account, context, work, proxyGroup);
                            }
                            else
                            {
                                await _scheduler.UpdateWorkAddPass(work);
                            }
                        }

                        await Logger.LogInformation(work, "Validations passed", account);

                        await _accountManager.UpdateAccount(account);

                        if (arguments.CustomBitmojiSelection != 0)
                        {
                            await Logger.LogInformation(work, "Attempting to set Custom BitMoji.", account);

                            await _runner.CreateCustomBitmoji(account, context,
                                Convert.ToInt32(arguments.CustomBitmojiSelection),
                                proxyGroup, work.CancellationTokenSource.Token);
                        }
                        else
                        {
                            if (arguments.BitmojiSelection != BitmojiSelection.None)
                            {
                                await Logger.LogInformation(work, "Attempting to set BitMoji.", account);

                                await SetBitmoji(_runner, account, work, arguments, proxyGroup);
                            }
                        }

                        return WorkStatus.Ok;
                    }

                    return WorkStatus.Error;
                }
            }
            catch (AggregateException ae)
            {
                var next = false;
                ae.Handle(e =>
                {
                    switch (e)
                    {
                        case TaskCanceledException:
                            return true;
                        case MaxRetriesException:
                            attempts++;
                            next = true;
                            Logger.LogError(work, e).Wait(work.CancellationTokenSource.Token);
                            _scheduler.FailWorkAccount(work, null).Wait(work.CancellationTokenSource.Token);
                            return true;
                        default:
                            // We will just report this error and continue the process for other accounts
                            Logger.LogError(work, e).Wait(work.CancellationTokenSource.Token);
                            _scheduler.FailWorkAccount(work, null).Wait(work.CancellationTokenSource.Token);
                            return false;
                    }
                });

                if (next)
                    continue;

                return WorkStatus.Error;
            }
            catch (Exception ex) when (ex is EmailTakenException || ex is EmailDomainBannedException)
            {
                await Logger.LogDebug(work,
                    $"CreateAccount Job {work.Id}: Account email is either taken or banned. Retrying");
                attempts += 1;

                if (email != null)
                {
                    // Pass on the address because I am not sure at this point if email is the right model or not
                    await _emailManager.DeleteEmailFromDatabase(email.Address);
                }
            }
            catch (TaskCanceledException)
            {
                // In this case, we don't want to report anything, so just return
                return WorkStatus.Cancelled;
            }
            catch (Exception ex)
            {
                // We will just report this error and continue the process for other accounts
                await Logger.LogError(work, ex);
                await _scheduler.FailWorkAccount(work, null);
                return WorkStatus.Error;
            }
            finally
            {
                
            }
        }

        if (created) return WorkStatus.Ok;

        if (work.CancellationTokenSource.Token.IsCancellationRequested) return WorkStatus.Cancelled;

        await Logger.LogError(work, "Unable to create account after 10 attempts");
        await _scheduler.FailWorkAccount(work, null);
        return WorkStatus.Error;
    }

    public async Task Start(WorkRequest work, CreateAccountArguments arguments, SnapchatActionsWorker actionsWorker)
    {
        try
        {
            await Logger.LogInformation(work, $"Starting CreateAccounts work with Id: {work.Id}");
            if (!await _scheduler.EndWorkForInvalidArguments(work, arguments)) return;

            var settings = _settingsLoader.Load().Result;

            var tasks = new List<Task<WorkStatus>>();
            // Try to create the total of accounts requested
            for (var i = 0; i < work.AccountsLeft; i++)
            {
                if (work.CancellationTokenSource.IsCancellationRequested) break;
                var task = new Task<WorkStatus>(() => DoWork(work, arguments, settings).Result);
                actionsWorker.QueueTask(new SnapchatTask { InnerTask = task, WorkRequest = work });
                tasks.Add(task);
            }

            await actionsWorker.WaitTasksCompletion(work, tasks, work.CancellationTokenSource.Token);

            // Because multiple threads act on a given work, the status in the DoWork function above does not properly mark
            // the work status. So for anything finishing with error, the status is not being bubbled up.
            // Instead, we'll check the results of every tasks to check for any errors. Otherwise, if a task is cancelled,
            // That should be handled by the scheduler since there's database interactions, as well as for the other statuses
            // DISCLAIMER: I DO NOT LIKE THIS RIGHT NOW

            if (tasks.Any(t => t.Result == WorkStatus.Error))
            {
                await _scheduler.EndWork(work, WorkStatus.Error);
            }
            else
            {
                await _scheduler.EndWork(work);
            }
        }
        catch (Exception ex)
        {
            await Logger.LogError(work, $"AccountCreation: {ex.Message}{ex.StackTrace}");
            await _scheduler.EndWork(work, WorkStatus.Error);
        }
    }
}