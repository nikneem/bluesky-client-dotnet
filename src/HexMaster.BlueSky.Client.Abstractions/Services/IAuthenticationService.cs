using System.Threading;
using System.Threading.Tasks;
using HexMaster.BlueSky.Client.Abstractions.Models.Authentication;

namespace HexMaster.BlueSky.Client.Abstractions.Services
{
    /// <summary>
    /// Interface for authentication services with the BlueSky API
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Creates a new authentication session with the BlueSky API
        /// </summary>
        /// <param name="identifier">The user identifier (username or email)</param>
        /// <param name="password">The user password</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A response containing session tokens and user information</returns>
        Task<CreateSessionResponse> CreateSessionAsync(string identifier, string password, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes an existing authentication session using a refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token from a previous session</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A response containing new session tokens and user information</returns>
        Task<CreateSessionResponse> RefreshSessionAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current session information
        /// </summary>
        /// <returns>The current session information, or null if not authenticated</returns>
        CreateSessionResponse? GetCurrentSession();

        /// <summary>
        /// Gets the current access token for authenticated requests
        /// </summary>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The current access token, refreshing if necessary</returns>
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
