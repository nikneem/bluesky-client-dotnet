using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HexMaster.BlueSky.Client.Abstractions.Models.Posts
{
    /// <summary>
    /// A model representing a BlueSky post with text content, facets, and embeds
    /// </summary>
    public class Post
    {
        /// <summary>
        /// The main text content of the post
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Collection of facets (mentions, links, hashtags) in the post
        /// </summary>
        [JsonPropertyName("facets")]
        public List<Facet>? Facets { get; set; }

        /// <summary>
        /// Embedded content such as images or links
        /// </summary>
        [JsonPropertyName("embed")]
        public Embed? Embed { get; set; }

        /// <summary>
        /// Reply reference if this post is a reply to another post
        /// </summary>
        [JsonPropertyName("reply")]
        public ReplyRef? Reply { get; set; }
    }
}
