using System;
using System.Threading.Tasks;
using HexMaster.BlueSky.Client.Abstractions.Configuration;
using HexMaster.BlueSky.Client.Abstractions.Services;
using HexMaster.BlueSky.Client.Configuration;
using HexMaster.BlueSky.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Http.Resilience;

namespace HexMaster.BlueSky.Client
{
    /// <summary>
    /// The main client for interacting with the BlueSky API
    /// </summary>
    public class BlueSkyClient
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// The authentication service for managing sessions
        /// </summary>
        public IAuthenticationService Authentication { get; }

        /// <summary>
        /// The posts service for creating and managing posts
        /// </summary>
        public IPostsService Posts { get; }

        /// <summary>
        /// The media service for uploading and attaching media
        /// </summary>
        public IMediaService Media { get; }

        /// <summary>
        /// Creates a new instance of the BlueSkyClient class with default options
        /// </summary>
        public BlueSkyClient() : this(new BlueSkyClientOptions())
        {
        }

        /// <summary>
        /// Creates a new instance of the BlueSkyClient class with custom options
        /// </summary>
        /// <param name="options">The configuration options</param>
        public BlueSkyClient(BlueSkyClientOptions options)
        {
            // Create a service collection and configure services
            var services = new ServiceCollection();

            // Configure options
            services.AddSingleton(Options.Create(options));

            // Configure logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();

                if (options.EnableLogging)
                {
                    builder.SetMinimumLevel(LogLevel.Debug);
                }
                else
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                }
            });

            // Register the session configuration as a singleton
            services.AddSingleton<SessionConfiguration>();            // Register the HTTP client with resilience
            services.AddHttpClient<IAuthenticationService, AuthenticationService>()
                .AddStandardResilienceHandler();

            services.AddHttpClient<IPostsService, PostsService>()
                .AddStandardResilienceHandler();

            services.AddHttpClient<IMediaService, MediaService>()
                .AddStandardResilienceHandler();

            // Register services
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IPostsService, PostsService>();
            services.AddSingleton<IMediaService, MediaService>();

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();

            // Resolve the services
            Authentication = _serviceProvider.GetRequiredService<IAuthenticationService>();
            Posts = _serviceProvider.GetRequiredService<IPostsService>();
            Media = _serviceProvider.GetRequiredService<IMediaService>();
        }

        /// <summary>
        /// Creates a session with the BlueSky API using username and password
        /// </summary>
        /// <param name="username">The username or email</param>
        /// <param name="password">The password</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task CreateSessionAsync(string username, string password)
        {
            return Authentication.CreateSessionAsync(username, password);
        }
    }
}
