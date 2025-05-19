using System;
using System.Net;

namespace HexMaster.BlueSky.Client.Abstractions.Exceptions
{
    /// <summary>
    /// Base exception for BlueSky API errors
    /// </summary>
    public class BlueSkyException : Exception
    {
        /// <summary>
        /// The HTTP status code returned by the BlueSky API
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// The error code returned by the BlueSky API
        /// </summary>
        public string? ErrorCode { get; }

        /// <summary>
        /// Creates a new instance of the BlueSkyException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="statusCode">The HTTP status code</param>
        /// <param name="errorCode">The BlueSky API error code</param>
        /// <param name="innerException">The inner exception</param>
        public BlueSkyException(string message, HttpStatusCode statusCode, string? errorCode = null, Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}
