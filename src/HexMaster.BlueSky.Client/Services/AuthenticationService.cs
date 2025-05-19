using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.BlueSky.Client.Abstractions.Configuration;
using HexMaster.BlueSky.Client.Abstractions.Exceptions;
using HexMaster.BlueSky.Client.Abstractions.Models.Authentication;
using HexMaster.BlueSky.Client.Abstractions.Services;
using HexMaster.BlueSky.Client.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HexMaster.BlueSky.Client.Services
{
    /// <summary>
    /// Implementation of the authentication service for the BlueSky API
    /// </summary>
    internal class AuthenticationService : BaseHttpService, IAuthenticationService
    {
        private readonly SessionConfiguration _sessionConfig;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);
        private readonly BlueSkyClientOptions _options;

        /// <summary>
        /// Creates a new instance of the AuthenticationService class
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests</param>
        /// <param name="options">The BlueSky client options</param>
        /// <param name="sessionConfig">The session configuration</param>
        /// <param name="logger">The logger</param>
        public AuthenticationService(
            HttpClient httpClient,
            IOptions<BlueSkyClientOptions> options,
            SessionConfiguration sessionConfig,
            ILogger<AuthenticationService> logger)
            : base(httpClient, null!, options.Value)
        {
            _sessionConfig = sessionConfig;
            _logger = logger;
            _options = options.Value;
        }

        /// <inheritdoc/>
        public async Task<CreateSessionResponse> CreateSessionAsync(string identifier, string password, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Creating BlueSky session for user {Identifier}", identifier);

                var request = new CreateSessionRequest
                {
                    Identifier = identifier,
                    Password = password
                };

                var response = await PostAsync<CreateSessionRequest, CreateSessionResponse>(
                    "com.atproto.server.createSession",
                    request,
                    requiresAuth: false,
                    cancellationToken);

                _sessionConfig.UpdateSession(response);

                _logger.LogInformation("Successfully created BlueSky session for user {Handle}", response.Handle);

                return response;
            }
            catch (BlueSkyException ex)
            {
                _logger.LogError(ex, "Failed to create BlueSky session: {Message}", ex.Message);
                throw new AuthenticationException("Failed to authenticate with BlueSky", ex.StatusCode, ex.ErrorCode, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<CreateSessionResponse> RefreshSessionAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Refreshing BlueSky session");

                var request = new RefreshSessionRequest
                {
                    RefreshJwt = refreshToken
                };

                var response = await PostAsync<RefreshSessionRequest, CreateSessionResponse>(
                    "com.atproto.server.refreshSession",
                    request,
                    requiresAuth: false,
                    cancellationToken);

                _sessionConfig.UpdateSession(response);

                _logger.LogInformation("Successfully refreshed BlueSky session");

                return response;
            }
            catch (BlueSkyException ex)
            {
                _logger.LogError(ex, "Failed to refresh BlueSky session: {Message}", ex.Message);
                _sessionConfig.ClearSession();
                throw new AuthenticationException("Failed to refresh session with BlueSky", ex.StatusCode, ex.ErrorCode, ex);
            }
        }

        /// <inheritdoc/>
        public CreateSessionResponse? GetCurrentSession()
        {
            return _sessionConfig.SessionInfo;
        }

        /// <inheritdoc/>
        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var session = _sessionConfig.SessionInfo;

            if (session == null)
            {
                throw new AuthenticationException("No active session. Please authenticate first.");
            }

            // If the token isn't expired, return it directly
            if (!_sessionConfig.IsAccessTokenExpired)
            {
                return session.AccessJwt;
            }

            // Don't attempt to refresh if auto-refresh is disabled
            if (!_options.AutoRefreshTokens)
            {
                throw new AuthenticationException("Session has expired and auto-refresh is disabled.");
            }

            // Use a semaphore to prevent multiple simultaneous refresh attempts
            await _refreshLock.WaitAsync(cancellationToken);

            try
            {
                // Check again after acquiring the lock in case another thread already refreshed
                if (!_sessionConfig.IsAccessTokenExpired)
                {
                    return _sessionConfig.SessionInfo!.AccessJwt;
                }

                _logger.LogDebug("Access token expired, refreshing...");

                var refreshedSession = await RefreshSessionAsync(session.RefreshJwt, cancellationToken);
                return refreshedSession.AccessJwt;
            }
            finally
            {
                _refreshLock.Release();
            }
        }
    }
}
