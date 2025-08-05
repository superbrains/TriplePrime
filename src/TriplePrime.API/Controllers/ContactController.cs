using Microsoft.AspNetCore.Mvc;
using TriplePrime.API.Models;
using TriplePrime.Data.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TriplePrime.API.Services;

namespace TriplePrime.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly NotificationService _notificationService;
        private readonly ILogger<ContactController> _logger;
        private const string CONTACT_EMAIL = "info@tripleprime.com.ng";

        public ContactController(
            IEmailService emailService, 
            NotificationService notificationService,
            ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ContactMessage message)
        {
            try
            {
                // Create email body
                var emailBody = $@"
                    <h2>New Contact Form Submission</h2>
                    <p><strong>From:</strong> {message.Name} ({message.Email})</p>
                    <p><strong>Subject:</strong> {message.Subject}</p>
                    <p><strong>Message:</strong></p>
                    <p>{message.Message}</p>
                ";

                // Send email
                await _emailService.SendEmailAsync(
                    CONTACT_EMAIL,
                    $"Contact Form: {message.Subject}",
                    emailBody
                );

                // Send confirmation email and push notification to the user
                var confirmationBody = $@"
                    <h2>Thank you for contacting TriplePrime</h2>
                    <p>Dear {message.Name},</p>
                    <p>We have received your message and will get back to you as soon as possible.</p>
                    <p>Here's a copy of your message:</p>
                    <p><strong>Subject:</strong> {message.Subject}</p>
                    <p><strong>Message:</strong></p>
                    <p>{message.Message}</p>
                    <p>Best regards,<br>TriplePrime Team</p>
                ";

                await _notificationService.SendEmailAndPushNotificationAsync(
                    message.Email,
                    "Thank you for contacting TriplePrime",
                    confirmationBody,
                    "Message Received",
                    "Thank you for contacting TriplePrime. We'll get back to you soon!"
                );

                return Ok(new { message = "Message sent successfully" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error sending contact message");
                return StatusCode(500, new { message = "Failed to send message. Please try again later." });
            }
        }
    }
} 