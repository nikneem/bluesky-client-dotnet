using System.Text.Json.Serialization;

namespace HexMaster.BlueSky.Client.Abstractions.Models.Posts
{
    /// <summary>
    /// Request model for creating a new post
    /// </summary>
    public class CreatePostRequest
    {
        /// <summary>
        /// The text content of the post
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Optional reply reference if this post is a reply
        /// </summary>
        public ReplyRef? Reply { get; set; }
    }

    /// <summary>
    /// Response for a successful post creation
    /// </summary>
    public class CreatePostResponse
    {
        /// <summary>
        /// The URI of the created post
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = null!;

        /// <summary>
        /// The content identifier of the created post
        /// </summary>
        [JsonPropertyName("cid")]
        public string Cid { get; set; } = null!;
    }
}
