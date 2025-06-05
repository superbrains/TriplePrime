using System;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Models;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace TriplePrime.Data.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;
        private readonly string _templateBasePath;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailService(
            ILogger<EmailService> logger,
            IOptions<EmailSettings> emailSettings,
            IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _emailSettings = emailSettings.Value;
            _webHostEnvironment = webHostEnvironment;
            _templateBasePath = Path.Combine(_webHostEnvironment.WebRootPath, _emailSettings.TemplatesPath);
            
            // Ensure the template directory exists
            if (!Directory.Exists(_templateBasePath))
            {
                Directory.CreateDirectory(_templateBasePath);
                _logger.LogInformation("Created email templates directory at: {TemplatesPath}", _templateBasePath);
            }
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var message = CreateEmailMessage(to, subject, body);
                await SendEmailMessageAsync(message);
                _logger.LogInformation("Email sent successfully to {EmailAddress}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {EmailAddress}", to);
                throw;
            }
        }

        public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath)
        {
            try
            {
                using var message = CreateEmailMessage(to, subject, body);
                
                if (File.Exists(attachmentPath))
                {
                    message.Attachments.Add(new Attachment(attachmentPath));
                }
                else
                {
                    throw new FileNotFoundException($"Attachment file not found: {attachmentPath}");
                }
                
                await SendEmailMessageAsync(message);
                _logger.LogInformation("Email with attachment sent successfully to {EmailAddress}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email with attachment to {EmailAddress}", to);
                throw;
            }
        }

        public async Task SendBulkEmailAsync(string[] recipients, string subject, string body)
        {
            try
            {
                if (recipients == null || !recipients.Any())
                {
                    throw new ArgumentException("Recipients list cannot be empty");
                }

                using var message = CreateEmailMessage(string.Empty, subject, body);
                foreach (var recipient in recipients)
                {
                    if (await ValidateEmailAsync(recipient))
                    {
                        message.Bcc.Add(recipient);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid email address skipped: {EmailAddress}", recipient);
                    }
                }

                if (!message.Bcc.Any())
                {
                    throw new InvalidOperationException("No valid recipients found");
                }
                
                await SendEmailMessageAsync(message);
                _logger.LogInformation("Bulk email sent successfully to {RecipientCount} recipients", message.Bcc.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk email to {RecipientCount} recipients", recipients.Length);
                throw;
            }
        }

        public async Task<bool> ValidateEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Basic email validation using regex
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                if (!emailRegex.IsMatch(email))
                    return false;

                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateEmailSettingsAsync(EmailSettings settings)
        {
            try
            {
                // Validate settings
                if (string.IsNullOrWhiteSpace(settings.SmtpServer) ||
                    string.IsNullOrWhiteSpace(settings.SmtpUsername) ||
                    string.IsNullOrWhiteSpace(settings.SmtpPassword))
                {
                    throw new ArgumentException("Required email settings are missing");
                }

                // Test the new settings by creating a connection
                using var client = CreateSmtpClient(settings);
                await client.SendMailAsync(new MailMessage()); // This will fail but test the connection
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating new email settings");
                return false;
            }
        }

        public async Task<EmailSettings> GetEmailSettingsAsync()
        {
            // Return a copy of settings to prevent modification
            return new EmailSettings
            {
                SmtpServer = _emailSettings.SmtpServer,
                SmtpPort = _emailSettings.SmtpPort,
                SmtpUsername = _emailSettings.SmtpUsername,
                SmtpPassword = "********", // Hide password for security
                SenderEmail = _emailSettings.SenderEmail,
                SenderName = _emailSettings.SenderName,
                EnableSsl = _emailSettings.EnableSsl,
                TemplatesPath = _emailSettings.TemplatesPath
            };
        }

        public async Task<string> GetEmailTemplateAsync(string templateName)
        {
            try
            {
                var templatePath = Path.Combine(_templateBasePath, templateName);
                _logger.LogInformation("Attempting to load template from: {TemplatePath}", templatePath);

                if (!File.Exists(templatePath))
                {
                    _logger.LogError("Email template not found at: {TemplatePath}", templatePath);
                    throw new FileNotFoundException($"Email template not found: {templateName}", templatePath);
                }

                var template = await File.ReadAllTextAsync(templatePath);
                if (string.IsNullOrWhiteSpace(template))
                {
                    _logger.LogError("Email template is empty: {TemplateName}", templateName);
                    throw new InvalidOperationException($"Email template is empty: {templateName}");
                }

                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading email template: {TemplateName}", templateName);
                throw;
            }
        }

        public async Task SendTemplatedEmailAsync(string to, string subject, string templateName, object model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(to))
                    throw new ArgumentException("Recipient email address is required");

                if (!await ValidateEmailAsync(to))
                    throw new ArgumentException($"Invalid recipient email address: {to}");

                var template = await GetEmailTemplateAsync(templateName);
                var body = ProcessTemplate(template, model);

                await SendEmailAsync(to, subject, body);
                _logger.LogInformation("Templated email sent successfully to {EmailAddress} using template {TemplateName}", 
                    to, templateName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated email to {EmailAddress} using template {TemplateName}", 
                    to, templateName);
                throw;
            }
        }

        private MailMessage CreateEmailMessage(string to, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            if (!string.IsNullOrEmpty(to))
            {
                message.To.Add(to);
            }

            return message;
        }

        private async Task SendEmailMessageAsync(MailMessage message)
        {
            using var client = CreateSmtpClient(_emailSettings);
            await client.SendMailAsync(message);
        }

        private SmtpClient CreateSmtpClient(EmailSettings settings)
        {
            var client = new SmtpClient(settings.SmtpServer, settings.SmtpPort)
            {
                EnableSsl = settings.EnableSsl,
                Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000 // 30 seconds timeout
            };

            return client;
        }

        private string ProcessTemplate(string template, object model)
        {
            if (model == null)
                return template;

            var processed = template;
            var properties = GetAllProperties(model);

            foreach (var prop in properties)
            {
                var placeholder = $"{{{{{prop.Key}}}}}";
                processed = processed.Replace(placeholder, prop.Value?.ToString() ?? string.Empty);
            }

            // Replace current year placeholder
            processed = processed.Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

            return processed;
        }

        private Dictionary<string, object> GetAllProperties(object obj)
        {
            var properties = new Dictionary<string, object>();

            if (obj == null) return properties;

            // Handle anonymous types and regular objects
            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = prop.GetValue(obj);
                properties.Add(prop.Name, value);

                // Handle nested objects (one level deep)
                if (value != null && !prop.PropertyType.IsPrimitive && prop.PropertyType != typeof(string))
                {
                    foreach (var nestedProp in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var nestedValue = nestedProp.GetValue(value);
                        properties.Add($"{prop.Name}.{nestedProp.Name}", nestedValue);
                    }
                }
            }

            return properties;
        }
    }
} 