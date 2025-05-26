using System;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Models;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace TriplePrime.Data.Services
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromPhoneNumber;

        public SmsService(IConfiguration configuration)
        {
            _configuration = configuration;
            _accountSid = _configuration["TwilioSettings:AccountSid"];
            _authToken = _configuration["TwilioSettings:AuthToken"];
            _fromPhoneNumber = _configuration["TwilioSettings:FromPhoneNumber"];

            TwilioClient.Init(_accountSid, _authToken);
        }

        public async Task SendSmsAsync(Notification notification)
        {
            if (string.IsNullOrEmpty(notification.RecipientPhone))
                throw new ArgumentException("Recipient phone number is required");

            var message = await MessageResource.CreateAsync(
                body: notification.Message,
                from: new PhoneNumber(_fromPhoneNumber),
                to: new PhoneNumber(notification.RecipientPhone)
            );

            if (message.Status != MessageResource.StatusEnum.Sent)
            {
                throw new Exception($"Failed to send SMS: {message.ErrorMessage}");
            }
        }

        public async Task SendSmsAsync(string recipientPhone, string message)
        {
            if (string.IsNullOrEmpty(recipientPhone))
                throw new ArgumentException("Recipient phone number is required");

            var sms = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromPhoneNumber),
                to: new PhoneNumber(recipientPhone)
            );

            if (sms.Status != MessageResource.StatusEnum.Sent)
            {
                throw new Exception($"Failed to send SMS: {sms.ErrorMessage}");
            }
        }

        public async Task SendBulkSmsAsync(string[] recipientPhones, string message)
        {
            if (recipientPhones == null || recipientPhones.Length == 0)
                throw new ArgumentException("Recipient phone numbers are required");

            foreach (var phone in recipientPhones)
            {
                try
                {
                    await SendSmsAsync(phone, message);
                }
                catch (Exception ex)
                {
                    // Log the error but continue with other recipients
                    Console.WriteLine($"Failed to send SMS to {phone}: {ex.Message}");
                }
            }
        }

        public async Task<string> GetMessageStatusAsync(string messageSid)
        {
            var message = await MessageResource.FetchAsync(messageSid);
            return message.Status.ToString();
        }

        public async Task<MessageResource> GetMessageDetailsAsync(string messageSid)
        {
            return await MessageResource.FetchAsync(messageSid);
        }

        public async Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
        {
            // Implementation will be added later
            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateSmsSettingsAsync(SmsSettings settings)
        {
            // Implementation will be added later
            return await Task.FromResult(true);
        }

        public async Task<SmsSettings> GetSmsSettingsAsync()
        {
            // Implementation will be added later
            return await Task.FromResult(new SmsSettings());
        }
    }
} 