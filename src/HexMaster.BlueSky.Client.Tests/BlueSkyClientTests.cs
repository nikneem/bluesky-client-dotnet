using System.Net;
using System.Text;
using System.Text.Json;
using HexMaster.BlueSky.Client.Abstractions.Models.Authentication;
using HexMaster.BlueSky.Client.Abstractions.Models.Posts;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.BlueSky.Client.Tests
{
    public class BlueSkyClientTests
    {
        [Fact]
        public async Task Client_CreateSession_Success()
        {
            // Arrange
            var mockMessageHandler = new MockHttpMessageHandler(req =>
            {
                if (req.RequestUri.ToString().Contains("createSession"))
                {
                    var response = new CreateSessionResponse
                    {
                        AccessJwt = "mock-access-token",
                        RefreshJwt = "mock-refresh-token",
                        Did = "did:plc:abcdef123456",
                        Handle = "test.user.bsky.app"
                    };

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            JsonSerializer.Serialize(response),
                            Encoding.UTF8,
                            "application/json")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddBlueSkyClient(options =>
            {
                options.BaseUrl = "https://test.bsky.app";
            });

            // Replace the HTTP client factory with our mock
            services.AddHttpClient<BlueSkyClient>(client =>
            {
                client.BaseAddress = new Uri("https://test.bsky.app");
            }).ConfigurePrimaryHttpMessageHandler(() => mockMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<BlueSkyClient>();

            // Act
            await client.CreateSessionAsync("test-user", "test-password");
            var session = client.Authentication.GetCurrentSession();

            // Assert
            Assert.NotNull(session);
            Assert.Equal("test.user.bsky.app", session.Handle);
            Assert.Equal("did:plc:abcdef123456", session.Did);
            Assert.Equal("mock-access-token", session.AccessJwt);
            Assert.Equal("mock-refresh-token", session.RefreshJwt);
        }

        [Fact]
        public async Task Client_CreateTextPost_Success()
        {
            // Arrange
            var mockMessageHandler = new MockHttpMessageHandler(req =>
            {
                if (req.RequestUri.ToString().Contains("createSession"))
                {
                    var response = new CreateSessionResponse
                    {
                        AccessJwt = "mock-access-token",
                        RefreshJwt = "mock-refresh-token",
                        Did = "did:plc:abcdef123456",
                        Handle = "test.user.bsky.app"
                    };

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            JsonSerializer.Serialize(response),
                            Encoding.UTF8,
                            "application/json")
                    };
                }
                else if (req.RequestUri.ToString().Contains("createRecord"))
                {
                    var response = new CreatePostResponse
                    {
                        Uri = "at://did:plc:abcdef123456/app.bsky.feed.post/test123",
                        Cid = "bafy1234567890"
                    };

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            JsonSerializer.Serialize(response),
                            Encoding.UTF8,
                            "application/json")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddBlueSkyClient(options =>
            {
                options.BaseUrl = "https://test.bsky.app";
            });

            // Replace the HTTP client factory with our mock
            services.AddHttpClient<BlueSkyClient>(client =>
            {
                client.BaseAddress = new Uri("https://test.bsky.app");
            }).ConfigurePrimaryHttpMessageHandler(() => mockMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<BlueSkyClient>();

            // First authenticate
            await client.CreateSessionAsync("test-user", "test-password");

            // Act
            var result = await client.Posts.CreateTextPostAsync("Hello from BlueSky .NET Client with #hashtag and @mention!");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("at://did:plc:abcdef123456/app.bsky.feed.post/test123", result.Uri);
            Assert.Equal("bafy1234567890", result.Cid);
        }
    }

    /// <summary>
    /// A mock HTTP message handler for testing
    /// </summary>
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseFactory(request));
        }
    }
}
