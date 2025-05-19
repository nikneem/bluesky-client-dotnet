using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.BlueSky.Client.Abstractions.Configuration;
using HexMaster.BlueSky.Client.Abstractions.Models.Posts;
using HexMaster.BlueSky.Client.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HexMaster.BlueSky.Client.Services
{
    /// <summary>
    /// Implementation of the posts service for the BlueSky API
    /// </summary>
    internal class PostsService : BaseHttpService, IPostsService
    {
        private readonly ILogger<PostsService> _logger;

        // Regular expressions for detecting mentions, hashtags, and URLs in post text
        private static readonly Regex MentionRegex = new Regex(@"@([a-zA-Z0-9.]+)", RegexOptions.Compiled);
        private static readonly Regex HashtagRegex = new Regex(@"#([a-zA-Z0-9_]+)", RegexOptions.Compiled);
        private static readonly Regex UrlRegex = new Regex(@"https?://\S+", RegexOptions.Compiled);

        /// <summary>
        /// Creates a new instance of the PostsService class
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests</param>
        /// <param name="authService">The authentication service</param>
        /// <param name="options">The BlueSky client options</param>
        /// <param name="logger">The logger</param>
        public PostsService(
            HttpClient httpClient,
            IAuthenticationService authService,
            IOptions<BlueSkyClientOptions> options,
            ILogger<PostsService> logger)
            : base(httpClient, authService, options.Value)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CreatePostResponse> CreateTextPostAsync(string text, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Creating text post with content: {TextPreview}",
                text.Length > 30 ? text.Substring(0, 30) + "..." : text);

            // Create a post with automatic detection of mentions, hashtags, and links
            var post = new Post { Text = text };

            // Detect and add facets for mentions, hashtags, and URLs
            post.Facets = DetectFacets(text);

            return await CreatePostAsync(post, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<CreatePostResponse> CreatePostAsync(Post post, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Creating post with content: {TextPreview}",
                post.Text.Length > 30 ? post.Text.Substring(0, 30) + "..." : post.Text);

            try
            {
                // Create the record in the format required by the API
                var request = new Dictionary<string, object>
                {
                    ["repo"] = await GetRepoNameAsync(cancellationToken),
                    ["collection"] = "app.bsky.feed.post",
                    ["record"] = new Dictionary<string, object>
                    {
                        ["$type"] = "app.bsky.feed.post",
                        ["text"] = post.Text,
                        ["createdAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    }
                };

                // Add facets if present
                if (post.Facets != null && post.Facets.Count > 0)
                {
                    var facets = post.Facets.Select(f => new Dictionary<string, object>
                    {
                        ["index"] = new Dictionary<string, int>
                        {
                            ["byteStart"] = f.Index.ByteStart,
                            ["byteEnd"] = f.Index.ByteEnd
                        },
                        ["features"] = f.Features.Select(ConvertFeatureToApiFormat).ToArray()
                    }).ToArray();

                    ((Dictionary<string, object>)request["record"]).Add("facets", facets);
                }

                // Add embed if present
                if (post.Embed != null)
                {
                    ((Dictionary<string, object>)request["record"]).Add("embed", ConvertEmbedToApiFormat(post.Embed));
                }

                // Add reply if present
                if (post.Reply != null)
                {
                    ((Dictionary<string, object>)request["record"]).Add("reply", new Dictionary<string, object>
                    {
                        ["root"] = new Dictionary<string, string>
                        {
                            ["uri"] = post.Reply.Root.Uri,
                            ["cid"] = post.Reply.Root.Cid
                        },
                        ["parent"] = new Dictionary<string, string>
                        {
                            ["uri"] = post.Reply.Parent.Uri,
                            ["cid"] = post.Reply.Parent.Cid
                        }
                    });
                }

                // Create the post
                var response = await PostAsync<Dictionary<string, object>, CreatePostResponse>(
                    "com.atproto.repo.createRecord",
                    request,
                    true,
                    cancellationToken);

                _logger.LogInformation("Successfully created post with URI: {Uri}", response.Uri);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create post: {Message}", ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CreatePostResponse> CreateReplyAsync(PostRef replyTo, string text, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Creating reply to post {Uri} with content: {TextPreview}",
                replyTo.Uri, text.Length > 30 ? text.Substring(0, 30) + "..." : text);

            var post = new Post
            {
                Text = text,
                Reply = new ReplyRef
                {
                    Root = replyTo,
                    Parent = replyTo
                },
                Facets = DetectFacets(text)
            };

            return await CreatePostAsync(post, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeletePostAsync(string uri, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Deleting post with URI: {Uri}", uri);

            try
            {
                // Extract the repo and recordName from the URI
                // URI format: at://did:plc:abc123/app.bsky.feed.post/recordName
                var parts = uri.Split('/');
                if (parts.Length < 4)
                {
                    throw new ArgumentException($"Invalid post URI format: {uri}");
                }

                var repo = parts[2];
                var recordName = parts[4];

                var request = new Dictionary<string, object>
                {
                    ["repo"] = repo,
                    ["collection"] = "app.bsky.feed.post",
                    ["rkey"] = recordName
                };

                await PostAsync("com.atproto.repo.deleteRecord", request, true, cancellationToken);

                _logger.LogInformation("Successfully deleted post with URI: {Uri}", uri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete post: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the repository name (DID) for the authenticated user
        /// </summary>
        private async Task<string> GetRepoNameAsync(CancellationToken cancellationToken)
        {
            var session = await GetSessionAsync(cancellationToken);
            return session.Did;
        }        /// <summary>
                 /// Gets the current session information
                 /// </summary>
        private Task<Abstractions.Models.Authentication.CreateSessionResponse> GetSessionAsync(CancellationToken cancellationToken)
        {
            var session = AuthService.GetCurrentSession();
            if (session == null)
            {
                _logger.LogError("No active session found");
                throw new Abstractions.Exceptions.AuthenticationException("No active session. Please authenticate first.");
            }
            return Task.FromResult(session);
        }

        /// <summary>
        /// Detects facets (mentions, hashtags, URLs) in the post text
        /// </summary>
        private List<Facet> DetectFacets(string text)
        {
            var facets = new List<Facet>();
            var textBytes = Encoding.UTF8.GetBytes(text);

            // Add mentions
            foreach (Match match in MentionRegex.Matches(text))
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
            foreach (Match match in HashtagRegex.Matches(text))
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
            foreach (Match match in UrlRegex.Matches(text))
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

        /// <summary>
        /// Converts a facet feature to the format expected by the API
        /// </summary>
        private object ConvertFeatureToApiFormat(FacetFeature feature)
        {
            if (feature is MentionFeature mention)
            {
                return new Dictionary<string, object>
                {
                    ["$type"] = "app.bsky.richtext.facet#mention",
                    ["did"] = mention.Did
                };
            }
            else if (feature is LinkFeature link)
            {
                return new Dictionary<string, object>
                {
                    ["$type"] = "app.bsky.richtext.facet#link",
                    ["uri"] = link.Uri
                };
            }
            else if (feature is TagFeature tag)
            {
                return new Dictionary<string, object>
                {
                    ["$type"] = "app.bsky.richtext.facet#tag",
                    ["tag"] = tag.Tag
                };
            }
            else
            {
                throw new ArgumentException($"Unknown facet feature type: {feature.GetType().Name}");
            }
        }

        /// <summary>
        /// Converts an embed to the format expected by the API
        /// </summary>
        private object ConvertEmbedToApiFormat(Embed embed)
        {
            if (embed is ImagesEmbed imagesEmbed)
            {
                var images = imagesEmbed.Images.Select(img => new Dictionary<string, object>
                {
                    ["alt"] = img.Alt,
                    ["image"] = new Dictionary<string, object>
                    {
                        ["$type"] = "blob",
                        ["ref"] = new Dictionary<string, string>
                        {
                            ["$link"] = img.ImageRef.Ref.Link
                        },
                        ["mimeType"] = img.ImageRef.MimeType,
                        ["size"] = img.ImageRef.Size
                    }
                }).ToArray();

                return new Dictionary<string, object>
                {
                    ["$type"] = "app.bsky.embed.images",
                    ["images"] = images
                };
            }
            else if (embed is ExternalEmbed externalEmbed)
            {
                var result = new Dictionary<string, object>
                {
                    ["$type"] = "app.bsky.embed.external",
                    ["external"] = new Dictionary<string, object>
                    {
                        ["uri"] = externalEmbed.Uri,
                    }
                };

                var external = (Dictionary<string, object>)((Dictionary<string, object>)result["external"]);

                if (!string.IsNullOrEmpty(externalEmbed.Title))
                {
                    external["title"] = externalEmbed.Title;
                }

                if (!string.IsNullOrEmpty(externalEmbed.Description))
                {
                    external["description"] = externalEmbed.Description;
                }

                if (externalEmbed.Thumb != null)
                {
                    external["thumb"] = new Dictionary<string, object>
                    {
                        ["$type"] = "blob",
                        ["ref"] = new Dictionary<string, string>
                        {
                            ["$link"] = externalEmbed.Thumb.Ref.Link
                        },
                        ["mimeType"] = externalEmbed.Thumb.MimeType,
                        ["size"] = externalEmbed.Thumb.Size
                    };
                }

                return result;
            }
            else
            {
                throw new ArgumentException($"Unknown embed type: {embed.GetType().Name}");
            }
        }
    }
}
