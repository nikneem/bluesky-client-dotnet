using System.Text.Json.Serialization;

namespace HexMaster.BlueSky.Client.Abstractions.Models.Authentication
{
    /// <summary>
    /// Request model for creating a new BlueSky session
    /// </summary>
    public class CreateSessionRequest
    {
        /// <summary>
        /// The identifier (username or email) of the BlueSky user
        /// </summary>
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; } = null!;

        /// <summary>
        /// The password of the BlueSky user
        /// </summary>
        [JsonPropertyName("password")]
        public string Password { get; set; } = null!;
    }
}
