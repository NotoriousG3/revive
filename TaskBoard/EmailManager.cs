using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using TaskBoard.Models;

namespace TaskBoard;

public class NoEmailAvailableException : Exception
{
    public NoEmailAvailableException() : base("There are no available e-mail accounts in the database")
    {
    }
}

public class LineParseErrorException : Exception
{
    public LineParseErrorException(int lineNumber) : base($"Index out of range when parsing line #{lineNumber}. Please validate that the line follows the correct format")
    {
    }
}

public class EmailManager
{
    private static readonly string[] _allowedEmailDomains = {"hotmail.com", "outlook.com", "gmx.com", "yandex.ru"};
    private readonly IServiceProvider _provider;
    private readonly ILogger<EmailManager> _logger;

    private HashSet<EmailModel> _emails = new();
    private HashSet<EmailModel> _loaned = new();

    public EmailManager(IServiceProvider provider, ILogger<EmailManager> logger)
    {
        _provider = provider;
        _logger = logger;
        Init();
    }

    public void Init()
    {
        using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _emails = context.Emails?.ToHashSet() ?? new HashSet<EmailModel>();
        _logger.LogDebug($"Initialized EmailManager with {_emails.Count} emails");
    } 

    private async Task WaitForAvailable(CancellationToken token)
    {
        while (_emails.Count == 0 && _loaned.Count > 0) await Task.Delay(1000, token);
    }

    /// <summary>
    ///     Get an available email from the database
    /// </summary>
    /// <returns></returns>
    public async Task<EmailModel> GetAvailable(CancellationToken token = default)
    {
        await WaitForAvailable(token);
        // Since we can have emails without a password, filter those because we wouldn't be able to connect
        var email = _emails.Count == 0 ? null : _emails.FirstOrDefault(e => !_loaned.Contains(e) && e.AccountId == null && !string.IsNullOrWhiteSpace(e.Password));
        
        if (email == null)
        {
            throw new NoEmailAvailableException();
        }

        // Append to _loaned since we don't know yet if it should be assigned to an account or not
        _loaned.Add(email);
        email.EmailManager = this;
        return email;
    }

    /// <summary>
    ///     Assign the specified account to the E-mail instance and save the changes to the DB
    /// </summary>
    /// <param name="account"></param>
    /// <param name="email"></param>
    public void AssignEmail(SnapchatAccountModel account, EmailModel email)
    {
        email.Account = account;
        
        // Remove this from _loaned since it is now used
        _loaned.Remove(email);
    }

    /// <summary>
    ///     Assign the specified account to the E-mail instance and save the changes to the DB
    /// </summary>
    /// <param name="account"></param>
    /// <param name="email"></param>
    /// <param name="context"></param>
    public async Task AssignEmail(SnapchatAccountModel account, EmailModel email, ApplicationDbContext context)
    {
        AssignEmail(account, email);
        context.Update(email);
        
        await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Move the reference of E-mail to the list of available addresses
    /// </summary>
    /// <param name="email"></param>
    public void ReleaseEmail(EmailModel email)
    {
        _loaned.Remove(email);

        // We only re-add if email is not assigned to an account
        if (email.Account != null) return;
        _emails.Add(email);
    }

    public async Task<EmailUploadResult> Import(string filePath)
    {
        using var scope = _provider.CreateScope();
        var emails = (await GetAllEmails()).ToImmutableHashSet();

        await using var stream = File.OpenRead(filePath);
        var reader = new StreamReader(stream);
        var addedEmails = new HashSet<EmailModel>();
        var duplicated = new List<EmailModel>();
        var rejected = new List<EmailRejectedReason>();

        var lineNumber = 0;
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();
            var fields = line.Split(':');

            EmailModel email;
            try
            {
                if (string.IsNullOrWhiteSpace(fields[0]))
                {
                    rejected.Add(new EmailRejectedReason {Email = fields[0], Reason = $"Line {lineNumber} - E-mail is empty"});
                    continue;
                }

                var utilities = scope.ServiceProvider.GetRequiredService<Utilities>();
                if (!utilities.IsValidEmail(fields[0]))
                {
                    rejected.Add(new EmailRejectedReason {Email = fields[0], Reason = "E-mail address is not valid"});
                    continue;
                }

                var domain = fields[0].Split("@")[1];
                if (!_allowedEmailDomains.Any(d => d.Equals(domain, StringComparison.OrdinalIgnoreCase)))
                {
                    rejected.Add(new EmailRejectedReason {Email = fields[0], Reason = $"E-mail domain is not allowed. Allowed domains are: {string.Join(", ", _allowedEmailDomains)}"});
                    continue;
                }

                email = new EmailModel
                {
                    Address = fields[0],
                    Password = fields.Length == 1 ? null : fields[1]
                };

                if (emails.Contains(email) || addedEmails.Contains(email))
                {
                    duplicated.Add(email);
                    continue;
                }

                addedEmails.Add(email);
            }
            catch (IndexOutOfRangeException)
            {
                throw new LineParseErrorException(lineNumber);
            }
        }

        if (addedEmails.Count > 0)
        {
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (context.Emails != null)
            {
                context.Emails.AddRange(addedEmails);
                await context.SaveChangesAsync();

                // Add this new range to the list of tracked emails
                _emails.AddRange(addedEmails);
            }
        }

        return new EmailUploadResult { Added = addedEmails, Duplicated = duplicated, Rejected = rejected };
    }

    public async Task<EmailModel?> GetEmailFromDatabase(string address)
    {
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return GetEmailFromDatabase(address, context);
    }

    private EmailModel? GetEmailFromDatabase(string address, ApplicationDbContext context)
    {
        if (context.Emails == null)
            throw new Exception("GetEmailFromDatabase is null handle this properly");

        return context.Emails.Include(e => e.Account).FirstOrDefault(t => t.Address.Equals(address));
    }

    public async Task<IEnumerable<EmailModel>> GetAllEmails()
    {
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Emails == null)
            throw new Exception("GetAllEmails is null handle this properly");

        return await context.Emails.ToListAsync();
    }

    public async Task<IEnumerable<EmailModel>> GetAccountsEmail(IEnumerable<SnapchatAccountModel> accounts)
    {
        var ids = accounts.Select(a => a.Id).ToList();
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Emails == null)
            throw new Exception("GetAccountsEmail is null handle this properly");

        return await context.Emails.Where(e => e.AccountId != null && ids.Contains((long)e.AccountId)).ToListAsync();
    }

    /// <summary>
    /// Find the provided address in the db, then delete the record AND its associated account if present
    /// </summary>
    /// <param name="address">email address to search and delete</param>
    public async Task DeleteEmailFromDatabase(string address)
    {
        await using var context = _provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var model = GetEmailFromDatabase(address, context);
        
        if (model != null)
        {
            if (model.AccountId != null)
            {
                // Also need to delete the account using this e-mail
                if(context.Accounts != null && model.Account != null)
                    context.Accounts.Remove(model.Account);
            }

            context.Emails?.Remove(model);

            await context.SaveChangesAsync();

            // This would remove anything that has the same address as model because EmailModel Equality is address
            _emails.Remove(model);
            _loaned.Remove(model);
        }
    }
}