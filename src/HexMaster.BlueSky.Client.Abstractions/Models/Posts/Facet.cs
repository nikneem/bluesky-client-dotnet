using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HexMaster.BlueSky.Client.Abstractions.Models.Posts
{
    /// <summary>
    /// A facet represents a portion of text in a post that can be formatted in a special way
    /// Examples include mentions, links, and hashtags
    /// </summary>
    public class Facet
    {
        /// <summary>
        /// The byte index range that this facet applies to in the post text
        /// </summary>
        [JsonPropertyName("index")]
        public ByteRange Index { get; set; } = new ByteRange();

        /// <summary>
        /// Features applied to the text range (like mentions, links, hashtags)
        /// </summary>
        [JsonPropertyName("features")]
        public List<FacetFeature> Features { get; set; } = new List<FacetFeature>();
    }

    /// <summary>
    /// A range of bytes in the post text where a facet applies
    /// </summary>
    public class ByteRange
    {
        /// <summary>
        /// The byte position where the facet starts (inclusive)
        /// </summary>
        [JsonPropertyName("byteStart")]
        public int ByteStart { get; set; }

        /// <summary>
        /// The byte position where the facet ends (exclusive)
        /// </summary>
        [JsonPropertyName("byteEnd")]
        public int ByteEnd { get; set; }
    }

    /// <summary>
    /// Base class for different types of facet features
    /// </summary>
    [JsonPolymorphic]
    [JsonDerivedType(typeof(MentionFeature), typeDiscriminator: "mention")]
    [JsonDerivedType(typeof(LinkFeature), typeDiscriminator: "link")]
    [JsonDerivedType(typeof(TagFeature), typeDiscriminator: "tag")]
    public abstract class FacetFeature
    {
        /// <summary>
        /// The type of facet feature
        /// </summary>
        [JsonPropertyName("$type")]
        public string Type { get; set; } = null!;
    }

    /// <summary>
    /// A mention feature that links to a BlueSky user
    /// </summary>
    public class MentionFeature : FacetFeature
    {
        /// <summary>
        /// The DID (Decentralized Identifier) of the mentioned user
        /// </summary>
        [JsonPropertyName("did")]
        public string Did { get; set; } = null!;
    }

    /// <summary>
    /// A link feature for clickable URLs in posts
    /// </summary>
    public class LinkFeature : FacetFeature
    {
        /// <summary>
        /// The URL that the link points to
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = null!;
    }

    /// <summary>
    /// A tag feature for hashtags in posts
    /// </summary>
    public class TagFeature : FacetFeature
    {
        /// <summary>
        /// The tag name without the # symbol
        /// </summary>
        [JsonPropertyName("tag")]
        public string Tag { get; set; } = null!;
    }
}
