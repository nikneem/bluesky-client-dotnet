using System.Text.Json.Serialization;

namespace HexMaster.BlueSky.Client.Abstractions.Models.Authentication
{
    /// <summary>
    /// Response model for a successful authentication session creation
    /// </summary>
    public class CreateSessionResponse
    {
        /// <summary>
        /// The access JWT token for authenticated API requests
        /// </summary>
        [JsonPropertyName("accessJwt")]
        public string AccessJwt { get; set; } = null!;

        /// <summary>
        /// The refresh JWT token for obtaining a new access token
        /// </summary>
        [JsonPropertyName("refreshJwt")]
        public string RefreshJwt { get; set; } = null!;

        /// <summary>
        /// The DID (Decentralized Identifier) of the authenticated user
        /// </summary>
        [JsonPropertyName("did")]
        public string Did { get; set; } = null!;

        /// <summary>
        /// The handle (username) of the authenticated user
        /// </summary>
        [JsonPropertyName("handle")]
        public string Handle { get; set; } = null!;
    }
}
