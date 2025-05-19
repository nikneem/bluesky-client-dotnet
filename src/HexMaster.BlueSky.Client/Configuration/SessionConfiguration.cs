using System;
using HexMaster.BlueSky.Client.Abstractions.Models.Authentication;

namespace HexMaster.BlueSky.Client.Configuration
{
    /// <summary>
    /// Class for managing the BlueSky session state
    /// </summary>
    internal class SessionConfiguration
    {
        private readonly object _lock = new object();
        private CreateSessionResponse? _sessionInfo;

        /// <summary>
        /// Gets the current session information
        /// </summary>
        public CreateSessionResponse? SessionInfo
        {
            get
            {
                lock (_lock)
                {
                    return _sessionInfo;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current access token is expired
        /// </summary>
        /// <remarks>
        /// JWT tokens don't expose their expiration directly in this library,
        /// but we'll use a conservative approach of considering them close to expiry
        /// after 50 minutes (tokens typically last 1 hour)
        /// </remarks>
        public bool IsAccessTokenExpired
        {
            get
            {
                lock (_lock)
                {
                    if (_sessionInfo == null || _lastTokenRefresh == DateTime.MinValue)
                    {
                        return true;
                    }

                    // Consider tokens expired after 50 minutes to be safe
                    return (DateTime.UtcNow - _lastTokenRefresh).TotalMinutes > 50;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user is authenticated
        /// </summary>
        public bool IsAuthenticated => SessionInfo != null;

        private DateTime _lastTokenRefresh = DateTime.MinValue;

        /// <summary>
        /// Updates the session information
        /// </summary>
        /// <param name="sessionInfo">The new session information</param>
        public void UpdateSession(CreateSessionResponse sessionInfo)
        {
            lock (_lock)
            {
                _sessionInfo = sessionInfo;
                _lastTokenRefresh = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Clears the session information
        /// </summary>
        public void ClearSession()
        {
            lock (_lock)
            {
                _sessionInfo = null;
                _lastTokenRefresh = DateTime.MinValue;
            }
        }
    }
}
