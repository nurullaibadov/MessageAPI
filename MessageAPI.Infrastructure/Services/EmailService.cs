using MailKit.Net.Smtp;
using MessageAPI.Application.Interfaces;
using MessageAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAPI.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings) => _settings = settings.Value;

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlBody };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, _settings.EnableSsl);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendPasswordResetEmailAsync(string to, string token, string email)
        {
            var resetLink = $"http://localhost:3000/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
            var body = $@"
        <h2>Password Reset Request</h2>
        <p>Click the button below to reset your password. This link expires in 1 hour.</p>
        <a href='{resetLink}' style='background:#007bff;color:white;padding:12px 24px;text-decoration:none;border-radius:4px;'>Reset Password</a>
        <p>If you didn't request this, please ignore this email.</p>";
            await SendEmailAsync(to, "Password Reset - ChatApp", body);
        }

        public async Task SendEmailVerificationAsync(string to, string token, string email)
        {
            var verifyLink = $"http://localhost:3000/verify-email?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
            var body = $@"
        <h2>Verify Your Email</h2>
        <p>Click below to verify your email address.</p>
        <a href='{verifyLink}' style='background:#28a745;color:white;padding:12px 24px;text-decoration:none;border-radius:4px;'>Verify Email</a>";
            await SendEmailAsync(to, "Verify Email - ChatApp", body);
        }

        public async Task SendWelcomeEmailAsync(string to, string username)
        {
            var body = $"<h2>Welcome to ChatApp, {username}!</h2><p>Your account has been created successfully.</p>";
            await SendEmailAsync(to, "Welcome to ChatApp!", body);
        }
    }
}
