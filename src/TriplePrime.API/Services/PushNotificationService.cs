using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.API.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly string _fcmUrl = "https://fcm.googleapis.com/v1/projects/{0}/messages:send";
        private readonly string _projectId;
        private readonly GoogleCredential _googleCredential;
        private readonly bool _isInitialized;

        public PushNotificationService(IConfiguration configuration)
        {
            // Get projectId from configuration
            _projectId = configuration["FirebaseSettings:ProjectId"]!;
            if (string.IsNullOrEmpty(_projectId))
            {
                Console.WriteLine("Warning: Firebase project ID is missing from configuration. Push notifications will be disabled.");
                _isInitialized = false;
                return;
            }

            // Try to initialize the service, but don't throw if it fails
            try
            {
                // Try multiple possible locations for the service account file
                var possiblePaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tripleprime-firebase-adminsdk.json"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "tripleprime-firebase-adminsdk.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "tripleprime-firebase-adminsdk.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tripleprime-firebase-adminsdk.json")
                };

                var serviceAccountPath = possiblePaths.FirstOrDefault(path => File.Exists(path));
                
                if (serviceAccountPath != null)
                {
                    // Load the service account credentials
                    _googleCredential = GoogleCredential.FromFile(serviceAccountPath)
                        .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
                    _isInitialized = true;
                    Console.WriteLine("PushNotificationService initialized successfully.");
                }
                else
                {
                    // Log that the service account file was not found
                    Console.WriteLine($"Warning: Firebase service account file not found in any of the expected locations: {string.Join(", ", possiblePaths)}");
                    _isInitialized = false;
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw
                Console.WriteLine($"Error initializing PushNotificationService: {ex.Message}");
                _isInitialized = false;
            }
        }

        public async Task<bool> SendNotificationAsync(List<string> recipientTokens, string title, string body, Dictionary<string, string> data = null)
        {
            if (!_isInitialized)
            {
                Console.WriteLine("PushNotificationService is not initialized. Skipping notification.");
                return false;
            }

            if (recipientTokens == null || recipientTokens.Count == 0)
            {
                Console.WriteLine("No recipient tokens provided. Skipping push notification.");
                return false;
            }

            // Filter out null or empty tokens
            var validTokens = recipientTokens.Where(token => !string.IsNullOrWhiteSpace(token)).ToList();
            if (validTokens.Count == 0)
            {
                Console.WriteLine("No valid recipient tokens provided. Skipping push notification.");
                return false;
            }

            // Build the FCM API URL
            var url = string.Format(_fcmUrl, _projectId);

            try
            {
                // Get an OAuth 2.0 access token
                var token = await GetAccessTokenAsync();

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Send to each token individually to handle failures gracefully
                var successCount = 0;
                foreach (var deviceToken in validTokens)
                {
                    var message = new
                    {
                        message = new
                        {
                            token = deviceToken,
                            notification = new
                            {
                                title,
                                body
                            },
                            data = data ?? new Dictionary<string, string>()
                        }
                    };

                    var jsonMessage = JsonConvert.SerializeObject(message);
                    var httpContent = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, httpContent);

                    if (response.IsSuccessStatusCode)
                    {
                        successCount++;
                        Console.WriteLine($"Push notification sent successfully to token: {deviceToken.Substring(0, Math.Min(10, deviceToken.Length))}...");
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error sending push notification to token {deviceToken.Substring(0, Math.Min(10, deviceToken.Length))}...: {errorResponse}");
                    }
                }

                Console.WriteLine($"Push notifications sent: {successCount}/{validTokens.Count} successful");
                return successCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending push notification: {ex.Message}");
                return false;
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var accessToken = await _googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            return accessToken;
        }
    }
}