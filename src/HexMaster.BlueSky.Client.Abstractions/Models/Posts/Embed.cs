using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HexMaster.BlueSky.Client.Abstractions.Models.Posts
{
    /// <summary>
    /// Embedded content that can be included in a post, such as images or external links
    /// </summary>
    [JsonPolymorphic]
    [JsonDerivedType(typeof(ImagesEmbed), typeDiscriminator: "images")]
    [JsonDerivedType(typeof(ExternalEmbed), typeDiscriminator: "external")]
    public abstract class Embed
    {
        /// <summary>
        /// The type of embed
        /// </summary>
        [JsonPropertyName("$type")]
        public string Type { get; set; } = null!;
    }

    /// <summary>
    /// Embedded images in a post
    /// </summary>
    public class ImagesEmbed : Embed
    {
        /// <summary>
        /// Collection of images embedded in the post
        /// </summary>
        [JsonPropertyName("images")]
        public List<Image> Images { get; set; } = new List<Image>();
    }    /// <summary>
         /// An image embedded in a post
         /// </summary>
    public class Image
    {
        /// <summary>
        /// The blob reference for the uploaded image
        /// </summary>
        [JsonPropertyName("image")]
        public BlobRef ImageRef { get; set; } = null!;

        /// <summary>
        /// Alt text for the image for accessibility
        /// </summary>
        [JsonPropertyName("alt")]
        public string Alt { get; set; } = string.Empty;
    }

    /// <summary>
    /// Embedded external content like a link preview
    /// </summary>
    public class ExternalEmbed : Embed
    {
        /// <summary>
        /// The external URI to embed
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = null!;

        /// <summary>
        /// Title of the external content
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Description of the external content
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Thumbnail image for the external content
        /// </summary>
        [JsonPropertyName("thumb")]
        public BlobRef? Thumb { get; set; }
    }

    /// <summary>
    /// Reference to a blob in the BlueSky blob store
    /// </summary>
    public class BlobRef
    {
        /// <summary>
        /// The content type of the blob
        /// </summary>
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = null!;

        /// <summary>
        /// The size of the blob in bytes
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }

        /// <summary>
        /// The blob reference link
        /// </summary>
        [JsonPropertyName("ref")]
        public BlobRefLink Ref { get; set; } = null!;
    }

    /// <summary>
    /// Link to a blob in the BlueSky blob store
    /// </summary>
    public class BlobRefLink
    {
        /// <summary>
        /// The link type
        /// </summary>
        [JsonPropertyName("$link")]
        public string Link { get; set; } = null!;
    }
}
