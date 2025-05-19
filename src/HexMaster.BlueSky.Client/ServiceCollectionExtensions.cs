using System;
using HexMaster.BlueSky.Client.Abstractions.Configuration;
using HexMaster.BlueSky.Client.Abstractions.Services;
using HexMaster.BlueSky.Client.Configuration;
using HexMaster.BlueSky.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;

namespace HexMaster.BlueSky.Client
{
    /// <summary>
    /// Extensions for registering BlueSky client services with dependency injection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds BlueSky client services to the service collection with default options
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddBlueSkyClient(this IServiceCollection services)
        {
            return services.AddBlueSkyClient(options => { });
        }

        /// <summary>
        /// Adds BlueSky client services to the service collection with options configured by an action
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">The action to configure options</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddBlueSkyClient(this IServiceCollection services, Action<BlueSkyClientOptions> configureOptions)
        {
            // Configure options
            services.Configure<BlueSkyClientOptions>(configureOptions);

            // Register the session configuration as a singleton
            services.AddSingleton<SessionConfiguration>();

            // Register the HTTP client factory with resilience
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
            services.AddSingleton<BlueSkyClient>();

            return services;
        }

        /// <summary>
        /// Adds BlueSky client services to the service collection with options from configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <param name="configSectionPath">The configuration section path</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddBlueSkyClient(
            this IServiceCollection services,
            IConfiguration configuration,
            string configSectionPath = "BlueSky")
        {
            // Configure options from configuration
            services.Configure<BlueSkyClientOptions>(configuration.GetSection(configSectionPath));

            // Register the session configuration as a singleton
            services.AddSingleton<SessionConfiguration>();

            // Register the HTTP client factory with resilience
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
            services.AddSingleton<BlueSkyClient>();

            return services;
        }
    }
}
