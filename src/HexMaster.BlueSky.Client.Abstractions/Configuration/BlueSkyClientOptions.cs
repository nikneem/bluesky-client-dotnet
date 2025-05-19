using System;

namespace HexMaster.BlueSky.Client.Abstractions.Configuration
{
    /// <summary>
    /// Configuration options for the BlueSky client
    /// </summary>
    public class BlueSkyClientOptions
    {
        /// <summary>
        /// The base URL for the BlueSky API
        /// </summary>
        public string BaseUrl { get; set; } = "https://bsky.social";

        /// <summary>
        /// The timeout for HTTP requests in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// A flag indicating whether to automatically refresh tokens when they expire
        /// </summary>
        public bool AutoRefreshTokens { get; set; } = true;

        /// <summary>
        /// The number of retries for failed requests
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// The initial delay for retries in milliseconds
        /// </summary>
        public int RetryInitialDelayMs { get; set; } = 1000;

        /// <summary>
        /// The maximum delay for retries in milliseconds
        /// </summary>
        public int RetryMaxDelayMs { get; set; } = 30000;

        /// <summary>
        /// A flag indicating whether to enable request and response logging
        /// </summary>
        public bool EnableLogging { get; set; } = false;
    }
}