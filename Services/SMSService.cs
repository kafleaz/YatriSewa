using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;  // Add this if you don't already have Newtonsoft.Json

namespace YatriSewa.Services
{
    public class SMSService : ISMSService
    {
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public SMSService(IConfiguration configuration)
        {
            // Read Infobip API settings from appsettings.json
            _apiKey = configuration["Infobip:ApiKey"] ?? throw new ArgumentNullException(nameof(_apiKey), "API key cannot be null");
            _baseUrl = configuration["Infobip:BaseUrl"] ?? throw new ArgumentNullException(nameof(_baseUrl), "Base URL cannot be null");
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // Check if phone number is null or empty
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    throw new ArgumentException("Phone number cannot be null or empty", nameof(phoneNumber));
                }

                // Add +977 country code if the phone number doesn't start with '+'
                if (!phoneNumber.StartsWith("+"))
                {
                    phoneNumber = "+977" + phoneNumber;
                }

                // Log the phone number to ensure it has been set correctly
                Console.WriteLine($"Sending SMS to: {phoneNumber}");

                // Step 1: Set up the RestClient with the Infobip base URL
                var options = new RestClientOptions(_baseUrl)
                {
                    Timeout = TimeSpan.FromMilliseconds(-1)  // Set Timeout instead of MaxTimeout
                };
                var client = new RestClient(options);

                // Step 2: Set up the RestRequest with the endpoint and method
                var request = new RestRequest("/sms/2/text/advanced", RestSharp.Method.Post);  // Explicit namespace usage for 'Method'

                // Step 3: Add the necessary headers
                request.AddHeader("Authorization", $"App {_apiKey}");  // Use the API key from the configuration
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "application/json");

                // Step 4: Define the message object
                var messageBody = new
                {
                    messages = new[]
                    {
                        new
                        {
                            destinations = new[]
                            {
                                new { to = phoneNumber }
                            },
                            from = "447491163443",  // Replace with your approved Sender ID or virtual long number
                            text = message
                        }
                    }
                };

                // Step 5: Serialize the object to JSON
                string body = JsonConvert.SerializeObject(messageBody);
                request.AddStringBody(body, DataFormat.Json);

                // Step 6: Execute the request asynchronously
                RestResponse response = await client.ExecuteAsync(request);

                // Log the response from Infobip for debugging
                Console.WriteLine($"Response from Infobip: {response.Content}");

                // Check if the response indicates success
                if (response.IsSuccessful)
                {
                    Console.WriteLine("SMS sent successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error sending SMS: {response.Content}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                Console.WriteLine($"Error sending SMS: {ex.Message}");
                return false;
            }
        }
    }
}
