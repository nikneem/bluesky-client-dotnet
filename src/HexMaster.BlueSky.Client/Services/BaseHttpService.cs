using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using HexMaster.BlueSky.Client.Abstractions.Configuration;
using HexMaster.BlueSky.Client.Abstractions.Exceptions;
using HexMaster.BlueSky.Client.Abstractions.Services;

namespace HexMaster.BlueSky.Client.Services
{
    /// <summary>
    /// Base class for HTTP services that communicate with the BlueSky API
    /// </summary>
    internal abstract class BaseHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthenticationService _authService;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Creates a new instance of the BaseHttpService class
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests</param>
        /// <param name="authService">The authentication service for obtaining access tokens</param>
        /// <param name="options">The BlueSky client options</param>
        protected BaseHttpService(HttpClient httpClient, IAuthenticationService authService, BlueSkyClientOptions options)
        {
            _httpClient = httpClient;
            _authService = authService;

            _httpClient.BaseAddress = new Uri(options.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Gets the authentication service
        /// </summary>
        protected IAuthenticationService AuthService => _authService;

        /// <summary>
        /// Sends a GET request to the specified endpoint
        /// </summary>
        /// <typeparam name="TResponse">The expected response type</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="requiresAuth">Whether the request requires authentication</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The deserialized response</returns>
        protected async Task<TResponse> GetAsync<TResponse>(
            string endpoint,
            bool requiresAuth = true,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            if (requiresAuth)
            {
                var token = await _authService.GetAccessTokenAsync(cancellationToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await HandleResponseAsync<TResponse>(response, cancellationToken);
        }

        /// <summary>
        /// Sends a POST request to the specified endpoint
        /// </summary>
        /// <typeparam name="TRequest">The request payload type</typeparam>
        /// <typeparam name="TResponse">The expected response type</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="payload">The request payload</param>
        /// <param name="requiresAuth">Whether the request requires authentication</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The deserialized response</returns>
        protected async Task<TResponse> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest payload,
            bool requiresAuth = true,
            CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonSerializer.Serialize(payload, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            if (requiresAuth)
            {
                var token = await _authService.GetAccessTokenAsync(cancellationToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await HandleResponseAsync<TResponse>(response, cancellationToken);
        }

        /// <summary>
        /// Sends a POST request to the specified endpoint without a response body
        /// </summary>
        /// <typeparam name="TRequest">The request payload type</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="payload">The request payload</param>
        /// <param name="requiresAuth">Whether the request requires authentication</param>
        /// <param name="cancellationToken">A cancellation token</param>
        protected async Task PostAsync<TRequest>(
            string endpoint,
            TRequest payload,
            bool requiresAuth = true,
            CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonSerializer.Serialize(payload, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            if (requiresAuth)
            {
                var token = await _authService.GetAccessTokenAsync(cancellationToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessResponseAsync(response, cancellationToken);
        }

        /// <summary>
        /// Handles the HTTP response and deserializes the content
        /// </summary>
        /// <typeparam name="T">The expected response type</typeparam>
        /// <param name="response">The HTTP response message</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The deserialized response</returns>
        private async Task<T> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            await EnsureSuccessResponseAsync(response, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                return JsonSerializer.Deserialize<T>(content, _jsonOptions)
                    ?? throw new BlueSkyException("Failed to deserialize response", response.StatusCode);
            }
            catch (JsonException ex)
            {
                throw new BlueSkyException("Failed to parse response", response.StatusCode, innerException: ex);
            }
        }

        /// <summary>
        /// Ensures that the response was successful, throws appropriate exceptions otherwise
        /// </summary>
        /// <param name="response">The HTTP response message</param>
        /// <param name="cancellationToken">A cancellation token</param>
        private async Task EnsureSuccessResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            // Try to extract error information from response
            string? errorCode = null;
            string message = "An error occurred while communicating with the BlueSky API";

            try
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);
                if (errorResponse != null)
                {
                    errorCode = errorResponse.Error;
                    message = errorResponse.Message ?? message;
                }
            }
            catch
            {
                // If we can't parse the error response, use the status code and content as is
                message = $"HTTP {(int)response.StatusCode}: {content}";
            }

            // Handle specific error cases
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    throw new AuthenticationException(message, response.StatusCode, errorCode);

                case HttpStatusCode.TooManyRequests:
                    int retryAfter = 60; // Default retry after 60 seconds

                    if (response.Headers.TryGetValues("Retry-After", out var retryValues) &&
                        int.TryParse(retryValues.FirstOrDefault(), out var parsedRetry))
                    {
                        retryAfter = parsedRetry;
                    }

                    throw new RateLimitException(message, retryAfter, errorCode);

                default:
                    throw new BlueSkyException(message, response.StatusCode, errorCode);
            }
        }

        /// <summary>
        /// Simple model for deserializing error responses
        /// </summary>
        private class ErrorResponse
        {
            [JsonPropertyName("error")]
            public string? Error { get; set; }

            [JsonPropertyName("message")]
            public string? Message { get; set; }
        }
    }
}
