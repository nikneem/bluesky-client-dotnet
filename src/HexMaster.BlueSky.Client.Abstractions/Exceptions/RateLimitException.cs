using System;
using System.Net;

namespace HexMaster.BlueSky.Client.Abstractions.Exceptions
{
    /// <summary>
    /// Exception thrown when a request is rate limited by the BlueSky API
    /// </summary>
    public class RateLimitException : BlueSkyException
    {
        /// <summary>
        /// The number of seconds to wait before retrying
        /// </summary>
        public int RetryAfterSeconds { get; }

        /// <summary>
        /// Creates a new instance of the RateLimitException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="retryAfterSeconds">The number of seconds to wait before retrying</param>
        /// <param name="errorCode">The BlueSky API error code</param>
        /// <param name="innerException">The inner exception</param>
        public RateLimitException(string message, int retryAfterSeconds, string? errorCode = null, Exception? innerException = null)
            : base(message, HttpStatusCode.TooManyRequests, errorCode, innerException)
        {
            RetryAfterSeconds = retryAfterSeconds;
        }
    }
}
