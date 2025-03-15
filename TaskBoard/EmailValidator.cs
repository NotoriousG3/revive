using System.Text.RegularExpressions;
using MailKit.Net.Pop3;
using TaskBoard.Models;

namespace TaskBoard;

public interface IEmailValidator
{
    public Task<ValidationStatus> Validate(SnapchatAccountModel account, EmailModel email, CancellationToken token);
}

public class EmailValidator
{
    private readonly ILogger<EmailValidator> _logger;
    private readonly IProxyManager _proxyManager;

    protected EmailValidator(ILogger<EmailValidator> logger, IProxyManager proxyManager)
    {
        _logger = logger;
        _proxyManager = proxyManager;
    }

    private string GetSnapchatConfirmationLink(string body)
    {
        var regex = new Regex(@"https:\/\/accounts\.snapchat\.com\/accounts\/confirm_email\?n=[\w]*");
        return regex.Match(body).Value;
    }

    protected async Task<ValidationStatus> ValidateEmail(SnapchatAccountModel account, string host, int port, bool useSsl, EmailModel email, CancellationToken token)
    {
        // we want to do this dance for 5 minutes tops
        //|-----------------------------------------------------------------------|
        //|    o   \ o /  _ o         __|    \ /     |__        o _  \ o /   o    |
        //|   /|\    |     /\   ___\o   \o    |    o/    o/__   /\     |    /|\   |
        //|   / \   / \   | \  /)  |    ( \  /o\  / )    |  (\  / |   / \   / \   |
        //|-----------------------------------------------------------------------|
        
        var attempts = 1;
        var waitTime = TimeSpan.FromSeconds(Math.Pow(2, attempts));
        var maxWaitTime = TimeSpan.FromMinutes(5);
        var startTime = DateTime.Now;

        while (true)
        {
            using var client = new Pop3Client();
            await client.ConnectAsync(host, port, useSsl, token);

            client.AuthenticationMechanisms.Remove("XOAUTH2");
            
            try
            {
                await client.AuthenticateAsync(email.Address, email.Password, token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{email.Address} - {ex.Message}");
                return ValidationStatus.FailedValidation;
            }
            
            // Get all messages
            var countAttempts = 0;
            var count = 0;
            bool isFailed = false;
            
            while(true){ 
                count = await client.GetMessageCountAsync(token);
                
                if (count != 0)
                {
                    break;
                }

                if (countAttempts++ >= 5)
                {
                    isFailed = true;
                    break;
                }

                await Task.Delay(5000, token);
            }

            if (isFailed)
            {
                return ValidationStatus.FailedValidation;
            }
            
            var allMessages = await client.GetMessagesAsync(0, count, token);

            // Order the messages by date, then filter those that do not have a snapchat link
            var filtered = allMessages.OrderByDescending(m => m.Date).Where(m => m.HtmlBody != null && !string.IsNullOrWhiteSpace(GetSnapchatConfirmationLink(m.HtmlBody))).ToList();

            if (filtered.Count == 0)
            {
                if (DateTime.Now - startTime >= maxWaitTime) 
                {
                    await client.DisconnectAsync(true, token);
                    return ValidationStatus.FailedValidation;
                }

                await Task.Delay(waitTime, token);
                waitTime = TimeSpan.FromSeconds(Math.Pow(2, ++attempts));
                await client.DisconnectAsync(true, token);
                continue;
            }

            // We use the first, this is already ordered to be latest first
            var first = filtered.FirstOrDefault();

            if (first != null)
            {
                var link = GetSnapchatConfirmationLink(first.HtmlBody);

                return await account.SnapClient.ValidateEmail(link, false);
            }
        }
    }
}