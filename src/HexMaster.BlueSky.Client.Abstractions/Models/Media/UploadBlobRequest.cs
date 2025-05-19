using System.IO;
using System.Text.Json.Serialization;

namespace HexMaster.BlueSky.Client.Abstractions.Models.Media
{
    /// <summary>
    /// Request model for uploading a blob to BlueSky's blob store
    /// </summary>
    public class UploadBlobRequest
    {
        /// <summary>
        /// The content of the blob as a stream
        /// </summary>
        [JsonIgnore]
        public Stream Content { get; set; } = null!;

        /// <summary>
        /// The MIME type of the blob
        /// </summary>
        [JsonIgnore]
        public string ContentType { get; set; } = null!;
    }

    /// <summary>
    /// Response model for a successful blob upload
    /// </summary>
    public class UploadBlobResponse
    {
        /// <summary>
        /// The blob reference that can be used in posts
        /// </summary>
        [JsonPropertyName("blob")]
        public BlobRef Blob { get; set; } = null!;
    }

    /// <summary>
    /// Reference to a blob in the BlueSky blob store
    /// </summary>
    public class BlobRef
    {
        /// <summary>
        /// The link reference to the blob
        /// </summary>
        [JsonPropertyName("$link")]
        public string Link { get; set; } = null!;
    }
}
