using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace TaskBoard;
public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;
    private readonly SnapWebManagerClient _managerClient;

    public EmailSender(SnapWebManagerClient managerClient, ILogger<EmailSender> logger)
    {
        _managerClient = managerClient;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var sendGridInfo = await _managerClient.GetSendGridInfo();
        if (sendGridInfo == null)
        {
            throw new Exception("Could not retrieve SendGrid information from manager");
        }

        if (string.IsNullOrWhiteSpace(sendGridInfo.ApiKey))
        {
            throw new Exception("SendGrid api key is null");
        }
        
        await Execute(sendGridInfo, subject, message, toEmail);
    }

    public async Task Execute(SendGridInfo sendGridInfo, string subject, string message, string toEmail)
    {
        var client = new SendGridClient(sendGridInfo.ApiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(sendGridInfo.Email, sendGridInfo.Name),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Disable click tracking.
        // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        msg.SetClickTracking(false, false);
        var response = await client.SendEmailAsync(msg);
        var content = await response.Body.ReadAsStringAsync();
        _logger.LogInformation(response.IsSuccessStatusCode 
            ? $"Email to {toEmail} queued successfully!"
            : $"Failure to send email to {toEmail}\n{content}");
    }
}