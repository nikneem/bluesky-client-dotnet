using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.BlueSky.Client.Abstractions.Models.Media;
using HexMaster.BlueSky.Client.Abstractions.Models.Posts;

namespace HexMaster.BlueSky.Client.Abstractions.Services
{
    /// <summary>
    /// Interface for media-related operations on the BlueSky platform
    /// </summary>
    public interface IMediaService
    {
        /// <summary>
        /// Uploads an image to the BlueSky blob store
        /// </summary>
        /// <param name="imageStream">The image content as a stream</param>
        /// <param name="contentType">The MIME type of the image</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A response containing the blob reference for the uploaded image</returns>
        Task<UploadBlobResponse> UploadImageAsync(Stream imageStream, string contentType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a post with a single image attachment
        /// </summary>
        /// <param name="text">The text content of the post</param>
        /// <param name="imageStream">The image content as a stream</param>
        /// <param name="contentType">The MIME type of the image</param>
        /// <param name="altText">Alternative text description of the image for accessibility</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A response containing the URI and CID of the created post</returns>
        Task<CreatePostResponse> CreatePostWithImageAsync(
            string text,
            Stream imageStream,
            string contentType,
            string altText,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a post with multiple image attachments
        /// </summary>
        /// <param name="text">The text content of the post</param>
        /// <param name="images">A collection of images with their metadata</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A response containing the URI and CID of the created post</returns>
        Task<CreatePostResponse> CreatePostWithMultipleImagesAsync(
            string text,
            IEnumerable<ImageUpload> images,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Model for uploading an image with metadata
    /// </summary>
    public class ImageUpload
    {
        /// <summary>
        /// The image content as a stream
        /// </summary>
        public Stream Content { get; set; } = null!;

        /// <summary>
        /// The MIME type of the image
        /// </summary>
        public string ContentType { get; set; } = null!;

        /// <summary>
        /// Alternative text description of the image for accessibility
        /// </summary>
        public string AltText { get; set; } = string.Empty;
    }
}
