using System.Threading;
using System.Threading.Tasks;
using HexMaster.BlueSky.Client.Abstractions.Models.Posts;

namespace HexMaster.BlueSky.Client.Abstractions.Services
{
    /// <summary>
    /// Interface for post-related operations on the BlueSky platform
    /// </summary>
    public interface IPostsService
    {
        /// <summary>
        /// Creates a new post with text content
        /// </summary>
        /// <param name="text">The text content of the post</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A response containing the URI and CID of the created post</returns>
        Task<CreatePostResponse> CreateTextPostAsync(string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new post with advanced options including mentions, hashtags, links, and embedded media
        /// </summary>
        /// <param name="post">The post model with all content and formatting</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A response containing the URI and CID of the created post</returns>
        Task<CreatePostResponse> CreatePostAsync(Post post, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a reply to an existing post
        /// </summary>
        /// <param name="replyTo">The reference to the post being replied to</param>
        /// <param name="text">The text content of the reply</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A response containing the URI and CID of the created reply</returns>
        Task<CreatePostResponse> CreateReplyAsync(PostRef replyTo, string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an existing post
        /// </summary>
        /// <param name="uri">The URI of the post to delete</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task DeletePostAsync(string uri, CancellationToken cancellationToken = default);
    }
}
