using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.BlueSky.Client.Abstractions.Configuration;
using HexMaster.BlueSky.Client.Abstractions.Exceptions;
using HexMaster.BlueSky.Client.Abstractions.Models.Media;
using HexMaster.BlueSky.Client.Abstractions.Models.Posts;
using HexMaster.BlueSky.Client.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HexMaster.BlueSky.Client.Services
{
    /// <summary>
    /// Implementation of the media service for the BlueSky API
    /// </summary>
    internal class MediaService : BaseHttpService, IMediaService
    {
        private readonly IPostsService _postsService;
        private readonly ILogger<MediaService> _logger;
        private readonly BlueSkyClientOptions _options;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Creates a new instance of the MediaService class
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests</param>
        /// <param name="authService">The authentication service</param>
        /// <param name="postsService">The posts service</param>
        /// <param name="options">The BlueSky client options</param>
        /// <param name="logger">The logger</param>
        public MediaService(
            HttpClient httpClient,
            IAuthenticationService authService,
            IPostsService postsService,
            IOptions<BlueSkyClientOptions> options,
            ILogger<MediaService> logger)
            : base(httpClient, authService, options.Value)
        {
            _postsService = postsService;
            _logger = logger;
            _options = options.Value;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <inheritdoc/>
        public async Task<UploadBlobResponse> UploadImageAsync(Stream imageStream, string contentType, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Uploading image with content type: {ContentType}", contentType);

            try
            {
                // Get token from the base AuthService property
                var token = await AuthService.GetAccessTokenAsync(cancellationToken);

                // Create a multipart form content
                var ms = new MemoryStream();
                await imageStream.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;

                var formContent = new MultipartFormDataContent
                {
                    { new StreamContent(ms), "file", "image" }
                };

                // Set the content type for the image
                var fileContent = formContent.First() as StreamContent;
                fileContent!.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                // Create a request to upload the blob
                var request = new HttpRequestMessage(HttpMethod.Post, "com.atproto.repo.uploadBlob")
                {
                    Content = formContent
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Send the request 
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(_options.BaseUrl),
                    Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
                };

                var response = await httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new BlueSkyException($"Failed to upload blob: {errorContent}", response.StatusCode);
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var blobResponse = JsonSerializer.Deserialize<UploadBlobResponse>(content, _jsonOptions)
                    ?? throw new BlueSkyException("Failed to parse blob upload response", response.StatusCode);

                _logger.LogInformation("Successfully uploaded image blob");
                return blobResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image: {Message}", ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CreatePostResponse> CreatePostWithImageAsync(
            string text,
            Stream imageStream,
            string contentType,
            string altText,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Creating post with image, text: {TextPreview}", text.Length > 30 ? text.Substring(0, 30) + "..." : text);

            try
            {
                // First upload the image
                var uploadResponse = await UploadImageAsync(imageStream, contentType, cancellationToken);

                // Create a post with the uploaded image
                var post = new Post
                {
                    Text = text,
                    Facets = DetectFacets(text),
                    Embed = new ImagesEmbed
                    {
                        Images = new List<Image>
                        {                            new Image
                            {
                                Alt = altText,
                                ImageRef = new Abstractions.Models.Posts.BlobRef
                                {
                                    MimeType = contentType,
                                    Size = (int)imageStream.Length,
                                    Ref = new BlobRefLink
                                    {
                                        Link = uploadResponse.Blob.Link
                                    }
                                }
                            }
                        }
                    }
                };

                return await _postsService.CreatePostAsync(post, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create post with image: {Message}", ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CreatePostResponse> CreatePostWithMultipleImagesAsync(
            string text,
            IEnumerable<ImageUpload> images,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Creating post with multiple images, text: {TextPreview}", text.Length > 30 ? text.Substring(0, 30) + "..." : text);

            try
            {
                var uploadedImages = new List<Image>();

                // Upload each image
                foreach (var imageUpload in images)
                {
                    var uploadResponse = await UploadImageAsync(
                        imageUpload.Content,
                        imageUpload.ContentType,
                        cancellationToken); uploadedImages.Add(new Image
                        {
                            Alt = imageUpload.AltText,
                            ImageRef = new Abstractions.Models.Posts.BlobRef
                            {
                                MimeType = imageUpload.ContentType,
                                Size = (int)imageUpload.Content.Length,
                                Ref = new BlobRefLink
                                {
                                    Link = uploadResponse.Blob.Link
                                }
                            }
                        });
                }

                // Create a post with the uploaded images
                var post = new Post
                {
                    Text = text,
                    Facets = DetectFacets(text),
                    Embed = new ImagesEmbed
                    {
                        Images = uploadedImages
                    }
                };

                return await _postsService.CreatePostAsync(post, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create post with multiple images: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Detects facets (mentions, hashtags, URLs) in the post text
        /// </summary>
        private List<Facet> DetectFacets(string text)
        {
            var facets = new List<Facet>();
            var mentionRegex = new Regex(@"@([a-zA-Z0-9.]+)");
            var hashtagRegex = new Regex(@"#([a-zA-Z0-9_]+)");
            var urlRegex = new Regex(@"https?://\S+");

            // Add mentions
            foreach (Match match in mentionRegex.Matches(text))
            {
                string username = match.Groups[1].Value;
                int startPosition = match.Index;
                int endPosition = match.Index + match.Length;

                facets.Add(new Facet
                {
                    Index = new ByteRange
                    {
                        ByteStart = Encoding.UTF8.GetByteCount(text.Substring(0, startPosition)),
                        ByteEnd = Encoding.UTF8.GetByteCount(text.Substring(0, endPosition))
                    },
                    Features = new List<FacetFeature>
                    {
                        new MentionFeature
                        {
                            Type = "app.bsky.richtext.facet#mention",
                            Did = username // Note: In a real implementation, we'd need to look up the DID for the username
                        }
                    }
                });
            }

            // Add hashtags
            foreach (Match match in hashtagRegex.Matches(text))
            {
                string tag = match.Groups[1].Value;
                int startPosition = match.Index;
                int endPosition = match.Index + match.Length;

                facets.Add(new Facet
                {
                    Index = new ByteRange
                    {
                        ByteStart = Encoding.UTF8.GetByteCount(text.Substring(0, startPosition)),
                        ByteEnd = Encoding.UTF8.GetByteCount(text.Substring(0, endPosition))
                    },
                    Features = new List<FacetFeature>
                    {
                        new TagFeature
                        {
                            Type = "app.bsky.richtext.facet#tag",
                            Tag = tag
                        }
                    }
                });
            }

            // Add URLs
            foreach (Match match in urlRegex.Matches(text))
            {
                string url = match.Value;
                int startPosition = match.Index;
                int endPosition = match.Index + match.Length;

                facets.Add(new Facet
                {
                    Index = new ByteRange
                    {
                        ByteStart = Encoding.UTF8.GetByteCount(text.Substring(0, startPosition)),
                        ByteEnd = Encoding.UTF8.GetByteCount(text.Substring(0, endPosition))
                    },
                    Features = new List<FacetFeature>
                    {
                        new LinkFeature
                        {
                            Type = "app.bsky.richtext.facet#link",
                            Uri = url
                        }
                    }
                });
            }

            return facets;
        }
    }
}
