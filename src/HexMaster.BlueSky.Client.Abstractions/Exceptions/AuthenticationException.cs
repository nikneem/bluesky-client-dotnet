using System;
using System.Net;

namespace HexMaster.BlueSky.Client.Abstractions.Exceptions
{
    /// <summary>
    /// Exception thrown when authentication with the BlueSky API fails
    /// </summary>
    public class AuthenticationException : BlueSkyException
    {
        /// <summary>
        /// Creates a new instance of the AuthenticationException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="statusCode">The HTTP status code</param>
        /// <param name="errorCode">The BlueSky API error code</param>
        /// <param name="innerException">The inner exception</param>
        public AuthenticationException(string message, HttpStatusCode statusCode = HttpStatusCode.Unauthorized, string? errorCode = null, Exception? innerException = null)
            : base(message, statusCode, errorCode, innerException)
        {
        }
    }
}
