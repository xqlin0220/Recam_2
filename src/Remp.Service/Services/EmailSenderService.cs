using MailKit.Net.Smtp;
using MailKit.Security;  
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;                
using Remp.Common.Utilities;
using Remp.Service.DTOs;
using Remp.Service.Interfaces;

namespace Remp.Service.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly SmtpSettings _smtpSettings;

    private readonly ILogger<EmailSenderService> _logger;

    // Constructor: DI container automatically injects these two dependencies
    public EmailSenderService(
        IOptions<SmtpSettings> smtpSettings, 
        ILogger<EmailSenderService> logger)
    {
        _smtpSettings = smtpSettings.Value; 
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailRequestDto request)
    {
        try
        {
            // 1. Build email content
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            email.To.Add(new MailboxAddress(string.Empty, request.To));
            if (!string.IsNullOrWhiteSpace(request.Cc))
                email.Cc.Add(new MailboxAddress(string.Empty, request.Cc));
            if (!string.IsNullOrWhiteSpace(request.ReplyTo))
                email.ReplyTo.Add(new MailboxAddress(string.Empty, request.ReplyTo));
            email.Subject = request.Title;
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = request.Body,                        // HTML 
                TextBody = StripHtml(request.Body)             // Text
            };
            email.Body = bodyBuilder.ToMessageBody();

            // 2. Connect to SMTP server and send
            using var smtpClient = new SmtpClient(); 
            await smtpClient.ConnectAsync(
                _smtpSettings.Host,
                _smtpSettings.Port,
                _smtpSettings.UseSsl
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTlsWhenAvailable
            );
            await smtpClient.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);

            // send + disconnect
            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(quit: true);

            _logger.LogInformation(
                "Email sent successfully to {Recipient} with subject '{Subject}'",
                request.To, request.Title);
        }
        catch (Exception ex)
        {
            // log error
            _logger.LogError(ex,
                "Failed to send email to {Recipient} with subject '{Subject}'",
                request.To, request.Title);

            throw;
        }
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        // Replace all HTML with empty string
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
    }

}