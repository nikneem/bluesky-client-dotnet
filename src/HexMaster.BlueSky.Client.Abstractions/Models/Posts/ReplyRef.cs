using System.Text.Json.Serialization;

namespace HexMaster.BlueSky.Client.Abstractions.Models.Posts
{
    /// <summary>
    /// Reference to a post that is being replied to
    /// </summary>
    public class ReplyRef
    {
        /// <summary>
        /// Reference to the root post in a thread
        /// </summary>
        [JsonPropertyName("root")]
        public PostRef Root { get; set; } = null!;

        /// <summary>
        /// Reference to the parent post being replied to
        /// </summary>
        [JsonPropertyName("parent")]
        public PostRef Parent { get; set; } = null!;
    }

    /// <summary>
    /// Reference to a specific post
    /// </summary>
    public class PostRef
    {
        /// <summary>
        /// The collection URI of the post
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = null!;

        /// <summary>
        /// The CID (Content Identifier) of the post
        /// </summary>
        [JsonPropertyName("cid")]
        public string Cid { get; set; } = null!;
    }
}
