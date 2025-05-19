using System.Text.Json.Serialization;

namespace HexMaster.BlueSky.Client.Abstractions.Models.Authentication
{
    /// <summary>
    /// Request model for refreshing an existing BlueSky session
    /// </summary>
    public class RefreshSessionRequest
    {
        /// <summary>
        /// The refresh JWT token obtained from the CreateSession response
        /// </summary>
        [JsonPropertyName("refreshJwt")]
        public string RefreshJwt { get; set; } = null!;
    }
}
