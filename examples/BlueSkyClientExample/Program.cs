using System;
using System.IO;
using System.Threading.Tasks;
using HexMaster.BlueSky.Client;
using HexMaster.BlueSky.Client.Abstractions.Models.Posts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlueSkyClientExample
{
    public class Program
    {
        private static IServiceProvider _serviceProvider;
        private static BlueSkyClient _client;

        public static async Task Main(string[] args)
        {
            // Set up configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Set up dependency injection
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // Add BlueSky client with configuration
            services.AddBlueSkyClient(options =>
            {
                options.BaseUrl = config.GetValue<string>("BlueSky:BaseUrl") ?? "https://bsky.social";
                options.EnableLogging = true;
                options.TimeoutSeconds = 60;
            });

            // Build service provider and get BlueSky client
            _serviceProvider = services.BuildServiceProvider();
            _client = _serviceProvider.GetRequiredService<BlueSkyClient>();

            // Run the example
            try
            {
                await RunExampleAsync(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static async Task RunExampleAsync(IConfiguration config)
        {
            // Get credentials from config
            string username = config.GetValue<string>("BlueSky:Username");
            string password = config.GetValue<string>("BlueSky:Password");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Please configure BlueSky:Username and BlueSky:Password in appsettings.json");
                return;
            }

            // Authenticate
            Console.WriteLine($"Authenticating as {username}...");
            await _client.CreateSessionAsync(username, password);
            var session = _client.Authentication.GetCurrentSession();
            Console.WriteLine($"Authenticated as {session.Handle} (DID: {session.Did})");

            // Create a simple text post
            Console.WriteLine("\nCreating a simple text post...");
            string simplePostText = $"Hello from BlueSky .NET Client! This is a test post at {DateTime.UtcNow:O}";
            var simplePost = await _client.Posts.CreateTextPostAsync(simplePostText);
            Console.WriteLine($"Created post with URI: {simplePost.Uri}");

            // Create a post with hashtags and mentions
            Console.WriteLine("\nCreating a post with hashtags and mentions...");
            string richPostText = $"Testing the #BlueSkyAPI #dotnet client by @{username}. Created at {DateTime.UtcNow:O}";
            var richPost = await _client.Posts.CreateTextPostAsync(richPostText);
            Console.WriteLine($"Created post with URI: {richPost.Uri}");

            // Test image uploading if an image file path is provided
            string imagePath = config.GetValue<string>("TestImagePath");
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                Console.WriteLine("\nUploading an image and creating a post with it...");
                using (var imageStream = File.OpenRead(imagePath))
                {
                    var mediaPost = await _client.Media.CreatePostWithImageAsync(
                        $"Testing image upload via BlueSky .NET Client at {DateTime.UtcNow:O}",
                        imageStream,
                        "image/jpeg", // adjust based on your image type
                        "A test image uploaded by BlueSky .NET Client"
                    );
                    Console.WriteLine($"Created post with image: {mediaPost.Uri}");
                }
            }

            // Clean up - delete the posts we created
            Console.WriteLine("\nCleaning up posts...");
            await _client.Posts.DeletePostAsync(simplePost.Uri);
            await _client.Posts.DeletePostAsync(richPost.Uri);

            Console.WriteLine("\nDone!");
        }
    }
}
