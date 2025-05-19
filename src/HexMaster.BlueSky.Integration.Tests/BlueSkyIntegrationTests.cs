
using System.Security.Authentication;
using HexMaster.BlueSky.Client;
using HexMaster.BlueSky.Client.Abstractions.Models.Posts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.BlueSky.Integration.Tests
{
    // These are integration tests that will connect to the actual BlueSky API
    // To run these tests, add a file appsettings.json with your BlueSky credentials
    // with the following format:
    // {
    //   "BlueSky": {
    //     "Username": "your-username",
    //     "Password": "your-password"
    //   }
    // }
    // Note: These tests are skipped by default to avoid unexpected API calls
    public class BlueSkyIntegrationTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _username;
        private readonly string _password;
        private readonly bool _runIntegrationTests;

        public BlueSkyIntegrationTests()
        {
            // Load configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            _username = config["BlueSky:Username"];
            _password = config["BlueSky:Password"];
            _runIntegrationTests = bool.TryParse(config["RunIntegrationTests"], out bool run) && run;

            // Set up service provider
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddBlueSkyClient(options =>
            {
                options.BaseUrl = "https://bsky.social";
                options.EnableLogging = true;
            });

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Authentication_WithValidCredentials_ShouldSucceed()
        {
            if (!_runIntegrationTests || string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
            {
                // Skip test if integration tests are disabled or credentials are missing
                return;
            }

            // Arrange
            var client = _serviceProvider.GetRequiredService<BlueSkyClient>();

            // Act
            await client.CreateSessionAsync(_username, _password);
            var session = client.Authentication.GetCurrentSession();

            // Assert
            Assert.NotNull(session);
            Assert.Equal(_username, session.Handle);
            Assert.NotNull(session.Did);
            Assert.NotNull(session.AccessJwt);
            Assert.NotNull(session.RefreshJwt);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Authentication_WithInvalidCredentials_ShouldFail()
        {
            if (!_runIntegrationTests)
            {
                // Skip test if integration tests are disabled
                return;
            }

            // Arrange
            var client = _serviceProvider.GetRequiredService<BlueSkyClient>();

            // Act & Assert
            await Assert.ThrowsAsync<AuthenticationException>(async () =>
                await client.CreateSessionAsync("invalid-user", "invalid-password"));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreateTextPost_AfterAuthentication_ShouldSucceed()
        {
            if (!_runIntegrationTests || string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
            {
                // Skip test if integration tests are disabled or credentials are missing
                return;
            }

            // Arrange
            var client = _serviceProvider.GetRequiredService<BlueSkyClient>();
            await client.CreateSessionAsync(_username, _password);

            // Create a unique test message to avoid duplicate post errors
            var testMessage = $"Testing BlueSky .NET Client at {DateTime.UtcNow:O} #{Guid.NewGuid():N}";

            // Act
            var result = await client.Posts.CreateTextPostAsync(testMessage);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Uri);
            Assert.NotNull(result.Cid);

            // Cleanup - Delete the test post
            await client.Posts.DeletePostAsync(result.Uri);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreatePostWithImage_AfterAuthentication_ShouldSucceed()
        {
            if (!_runIntegrationTests || string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
            {
                // Skip test if integration tests are disabled or credentials are missing
                return;
            }

            // Skip this test if the test image doesn't exist
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "test-image.jpg");
            if (!File.Exists(imagePath))
            {
                return;
            }

            // Arrange
            var client = _serviceProvider.GetRequiredService<BlueSkyClient>();
            await client.CreateSessionAsync(_username, _password);

            // Create a unique test message to avoid duplicate post errors
            var testMessage = $"Testing BlueSky .NET Client with image at {DateTime.UtcNow:O} #{Guid.NewGuid():N}";

            // Act
            CreatePostResponse result;
            using (var imageStream = File.OpenRead(imagePath))
            {
                result = await client.Media.CreatePostWithImageAsync(
                    testMessage,
                    imageStream,
                    "image/jpeg",
                    "Test image uploaded by BlueSky .NET Client"
                );
            }

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Uri);
            Assert.NotNull(result.Cid);

            // Cleanup - Delete the test post
            await client.Posts.DeletePostAsync(result.Uri);
        }
    }
}
