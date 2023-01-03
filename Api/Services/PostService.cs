using Api.Configs;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Api.Models.User;
using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Post;
using Api.Models.PostContent;
using Microsoft.AspNetCore.Routing;
using System.IO;
using System.Linq;
using Api.Models.Relation;
using Api.Exceptions;
using Common.Enums;
using Common.Consts;

namespace Api.Services
{
    public class PostService : IDisposable
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        public Func<string, Guid>? UserId { get; set; }

        public PostService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<AttachModel> GetPostContent(Guid userId, Guid postContentId)
        {
            
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var content = await _context.PostContent.Include(x => x.Author)
                                                        .ThenInclude(x => x.Followers.Where(y => y.FollowerId == userId))
                                                    .FirstOrDefaultAsync(x => x.Id == postContentId && x.IsActive);
            if (content == null)
                throw new ContentNotFoundException();
            if (!content.Author.PrivateAccount && content.Author.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
            || content.Author.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
            || userId == content.AuthorId)
                return _mapper.Map<AttachModel>(content);
            throw new UserDontHaveAccessException();
        }

        public async Task CreatePost(CreatePostRequest request, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var postModel = _mapper.Map<CreatePostModel>(request);
            postModel.AuthorId = userId;
            var post = _mapper.Map<Post>(postModel);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task<PostModel> GetPost(Guid userId, Guid postId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new UserNotFoundException();
            var post = await _context.Posts.AsNoTracking()
                                           .Include(x => x.Author)
                                               .ThenInclude(x => x.Avatar)
                                            .Include(x => x.Author)
                                               .ThenInclude(x => x.Followers.Where(y => y.FollowerId == userId))
                                           .Include(x => x.Comments.Where(y => y.IsActive))
                                               .ThenInclude(x => x.Likes)
                                           .Include(x => x.Comments.Where(y => y.IsActive))
                                               .ThenInclude(x => x.Author)
                                                    .ThenInclude(x => x.Avatar)
                                           .Include(x => x.Content.Where(y => y.IsActive))
                                               .ThenInclude(x => x.Likes)
                                           .Include(x => x.Likes)
                                           .FirstOrDefaultAsync(x => x.Id == postId && x.IsActive);
            if (post == null)
                throw new PostNotFoundException();
            if (!post.Author.PrivateAccount && post.Author.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
                || post.Author.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
                || userId == post.AuthorID)
                post.Comments = post.Comments.OrderBy(x => x.Created).ToList();
            ; return _mapper.Map<PostModel>(post);
            throw new UserDontHaveAccessException();
        }

        public async Task<List<PostModel>> GetPostsByUserId(Guid userId, Guid targetUserId, int skip, int take)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var targetUser = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == userId))
                                                 .FirstOrDefaultAsync(x => x.Id == targetUserId && x.IsActive);
            if (targetUser == default)
                throw new UserNotFoundException();
            if (!targetUser.PrivateAccount && targetUser.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
                || targetUser.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
                || userId == targetUserId)
            {
                var posts = await _context.Posts.AsNoTracking()
                        .Where(x => x.AuthorID == targetUserId && x.IsActive)
                        .Include(x => x.Author)
                            .ThenInclude(x => x.Avatar)
                        .Include(x => x.Comments.Where(y => y.IsActive))
                            .ThenInclude(x => x.Likes)
                        .Include(x => x.Comments.Where(y => y.IsActive))
                            .ThenInclude(x => x.Author)
                        .Include(x => x.Content.Where(y => y.IsActive))
                            .ThenInclude(x => x.Likes)
                        .Include(x => x.Likes)
                        .OrderByDescending(x => x.Created)
                        .Skip(skip)
                        .Take(take)
                        .Select(x => _mapper.Map<PostModel>(x))
                        .ToListAsync();
                return posts;
            }
            throw new UserDontHaveAccessException();
        }

        public async Task<List<PostModel>> GetPostFeed(Guid userId, int skip, int take)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive == true))
                throw new UserNotFoundException();
            var followedUsersId = await _context.Relations.AsNoTracking()
                                                            .Where(x => x.FollowerId == userId && x.Followed.IsActive && x.State == RelationState.Follower.ToString())
                                                            .Select(x => x.FollowedId)
                                                            .ToListAsync();
            var result = await _context.Posts.AsNoTracking()
                                            .Where(x => followedUsersId.Contains(x.AuthorID) && x.IsActive)
                                            .Include(x => x.Author)
                                                .ThenInclude(x => x.Avatar)
                                            .Include(x => x.Comments.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Likes)
                                            .Include(x => x.Comments.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Author)
                                            .Include(x => x.Content.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Likes)
                                            .Include(x => x.Likes)
                                            .OrderByDescending(x => x.Created)
                                            .Skip(skip)
                                            .Take(take)
                                            .Select(x => _mapper.Map<PostModel>(x))
                                            .ToListAsync();
            return result;
        }



        public async Task<List<PostModel>> GetPostFeedByLastId(Guid userId, Guid? lastPostId) //TODO:custom
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive == true))
                throw new UserNotFoundException();
            var followedUsersId = await _context.Relations.AsNoTracking()
                                                            .Where(x => x.FollowerId == userId && x.Followed.IsActive && x.State == RelationState.Follower.ToString())
                                                            .Select(x => x.FollowedId)
                                                            .ToListAsync();
            var result = await _context.Posts.AsNoTracking()
                                            .Where(x => followedUsersId.Contains(x.AuthorID) && x.IsActive)
                                            .Include(x => x.Author)
                                                .ThenInclude(x => x.Avatar)
                                            .Include(x => x.Comments.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Likes)
                                            .Include(x => x.Comments.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Author)
                                            .Include(x => x.Content.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Likes)
                                            .Include(x => x.Likes)
                                            .OrderByDescending(x => x.Created)
                                            .SkipWhile(x =>/* lastPostId != null && */x.Id != lastPostId)
                                            .Take(3)
                                            .Select(x => _mapper.Map<PostModel>(x))
                                            .ToListAsync();
            return result;
        }



        public async Task<List<PostModel>> GetPostFeedByLastPostDate(Guid userId, String? lastPostDate) //TODO:custom
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive == true))
                throw new UserNotFoundException();
            var followedUsersId = await _context.Relations.AsNoTracking()
                                                            .Where(x => x.FollowerId == userId && x.Followed.IsActive && x.State == RelationState.Follower.ToString())
                                                            .Select(x => x.FollowedId)
                                                            .ToListAsync();

            var date = lastPostDate == null ? DateTimeOffset.UtcNow : DateTimeOffset.Parse(lastPostDate);

            var result = await _context.Posts.AsNoTracking()
                                            .Where(x => followedUsersId.Contains(x.AuthorID) && x.IsActive && x.Created < date)
                                            .Include(x => x.Author)
                                                .ThenInclude(x => x.Avatar)
                                            .Include(x => x.Comments.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Likes)
                                            .Include(x => x.Comments.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Author)
                                                    .ThenInclude(x => x.Avatar)
                                            .Include(x => x.Content.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Likes)
                                            .Include(x => x.Likes)
                                            .OrderByDescending(x => x.Created)
                                            .Take(5)
                                            .Select(x => _mapper.Map<PostModel>(x))
                                            .ToListAsync();

            result.ForEach(x => x.Comments = x.Comments!.OrderBy(y => y.Created).ToList());
            return result;
        }



        public async Task<List<PostModel>> GetPostsByLastPostDate(Guid userId, GetPostsRequestModel model) //TODO:custom
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive == true))
                throw new UserNotFoundException();

            var date = model.LastPostDate == null ? DateTimeOffset.UtcNow : DateTimeOffset.Parse(model.LastPostDate);

            var targetUser = await _context.Users.AsNoTracking()
                .Include(x => x.Followers.Where(y => y.FollowerId == userId))
                .FirstOrDefaultAsync(x => x.IsActive && x.Id == model.UserId);

            if (targetUser != null && (!targetUser.PrivateAccount && targetUser.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
    || targetUser.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
    || userId == model.UserId))
            {

                var result = await _context.Posts.AsNoTracking()
                                                .Where(x => x.AuthorID == model.UserId && x.IsActive && x.Created < date)
                                                .Include(x => x.Author)
                                                    .ThenInclude(x => x.Avatar)
                                                .Include(x => x.Comments.Where(y => y.IsActive))
                                                    .ThenInclude(x => x.Likes)
                                                .Include(x => x.Comments.Where(y => y.IsActive))
                                                    .ThenInclude(x => x.Author)
                                                        .ThenInclude(x => x.Avatar)
                                                .Include(x => x.Content.Where(y => y.IsActive))
                                                    .ThenInclude(x => x.Likes)
                                                .Include(x => x.Likes)
                                                .OrderByDescending(x => x.Created)
                                                .Take(model.postsAmount)
                                                .Select(x => _mapper.Map<PostModel>(x))
                                                .ToListAsync();

                result.ForEach(x => x.Comments = x.Comments!.OrderBy(y => y.Created).ToList());
                return result;
            }
            return new List<PostModel>();
        }

        public async Task<List<PostModel>> GetFavoritePosts(Guid userId, GetPostsRequestModel model)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive == true))
                throw new UserNotFoundException();

            var date = model.LastPostDate == null ? DateTimeOffset.UtcNow : DateTimeOffset.Parse(model.LastPostDate);
            var likedPostsId = await _context.PostLikes.AsNoTracking()
                                                        .Include(x => x.Post)
                                                        .Where(x => x.UserId == userId && x.Created < date && x.Post.IsActive)
                                                        .OrderByDescending(x => x.Created)
                                                        .Take(model.postsAmount)
                                                        .Select(x => x.PostId)
                                                        .ToListAsync();

            var result = await _context.Posts.AsNoTracking()
                                            .Where(x => likedPostsId.Contains(x.Id))
                                            .Include(x => x.Author)
                                                .ThenInclude(x => x.Avatar)
                                            .Include(x => x.Comments.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Likes)
                                            .Include(x => x.Comments.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Author)
                                                    .ThenInclude(x => x.Avatar)
                                            .Include(x => x.Content.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Likes)
                                            .Include(x => x.Likes)
                                            .OrderByDescending(x => x.Likes.First(y => y.UserId == userId).Created)
                                            .Take(model.postsAmount)
                                            .Select(x => _mapper.Map<PostModel>(x))
                                            .ToListAsync();

            result.ForEach(x => x.Comments = x.Comments!.OrderBy(y => y.Created).ToList());
            return result;
        }

        public async Task ChangePostDescription(ChangePostDescriptionModel model, Guid userId)
        {
            var user = await _context.Users.Include(x => x.Posts.Where(y => y.Id == model.PostId && y.IsActive))
                                           .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new UserNotFoundException();
            if (user.Posts.Count == 0)
                throw new PostNotFoundException();
            user.Posts.First().Description = model.Description;
            user.Posts.First().Changed = true;
            await _context.SaveChangesAsync();
        }

        public async Task DeletePost(Guid postId, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var post = await _context.Posts.Include(x => x.Author)
                                            .Include(x => x.Comments)
                                            .Include(x => x.Content)
                                            .FirstOrDefaultAsync(x => x.Id == postId && x.IsActive);
            if (post == null)
                throw new PostNotFoundException();
            if (post.Author.Id != userId)
                throw new UserDontHaveAccessException();
            post.IsActive = false;
            post.Comments.ToList().ForEach(x => x.IsActive = false);
            post.Content.ToList().ForEach(x => x.IsActive = false);
            await _context.SaveChangesAsync();
        }

        public async Task RestorePost(Guid postId, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var post = await _context.Posts.Include(x => x.Author)
                                            .Include(x => x.Comments)
                                            .Include(x => x.Content)
                                            .FirstOrDefaultAsync(x => x.Id == postId && !x.IsActive);
            if (post == null)
                throw new PostNotFoundException();
            //if (post.Author.Id != userId)
            //    throw new UserDontHaveAccessException();
            post.IsActive = true;
            post.Comments.ToList().ForEach(x => x.IsActive = true);
            post.Content.ToList().ForEach(x => x.IsActive = true);
            await _context.SaveChangesAsync();
        }

        public async Task CreateComment(CreateComment commentRequest, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var post = await _context.Posts.Include(x => x.Author)
                                                .ThenInclude(x => x.Followers.Where(y => y.FollowerId == userId))
                                            .FirstOrDefaultAsync(x => x.Id == commentRequest.PostId && x.IsActive);
            if (post == null)
                throw new PostNotFoundException();
            if (!post.Author.PrivateAccount && post.Author.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
                || post.Author.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
                || userId == post.AuthorID)
            {
                var comment = _mapper.Map<Comment>(commentRequest);
                comment.AuthorId = userId;
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
            }
            else
                throw new UserDontHaveAccessException();
        }

        public async Task<CommentModel> GetComment(Guid userId, Guid commentId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var comment = await _context.Comments.AsNoTracking()
                                                 .Include(x => x.Likes)
                                                 .Include(x => x.Author)
                                                    .ThenInclude(x => x.Avatar)
                                                 .Include(x => x.Post)
                                                    .ThenInclude(x => x.Author)
                                                        .ThenInclude(x => x.Followers.Where(y => y.FollowerId == userId))
                                                 .FirstOrDefaultAsync(x => x.Id == commentId && x.IsActive);
            if (comment == null)
                throw new CommentNotFoundException();
            if (!comment.Post.Author.PrivateAccount && comment.Post.Author.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
                || comment.Post.Author.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
                || userId == comment.AuthorId)
            {
                return _mapper.Map<CommentModel>(comment);
            }
            else
                throw new UserDontHaveAccessException();
        }

        public async Task<List<CommentModel>> GetComments(Guid userId, Guid postId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var post = await _context.Posts.AsNoTracking()
                                            .Include(x => x.Author)
                                                .ThenInclude(x => x.Followers.Where(y => y.FollowerId == userId))
                                            .Include(x => x.Comments.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Likes)
                                            .Include(x => x.Comments.Where(y => y.IsActive))
                                                .ThenInclude(x => x.Author)
                                                    .ThenInclude(x => x.Avatar)
                                            .FirstOrDefaultAsync(x => x.Id == postId && x.IsActive);
            if (post == null)
                throw new PostNotFoundException();
            if (!post.Author.PrivateAccount && post.Author.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
                || post.Author.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
                || userId == post.AuthorID)
                return post.Comments.OrderBy(x => x.Created)
                    .Select(x => _mapper.Map<CommentModel>(x))
                    .ToList();
            else
                throw new UserDontHaveAccessException();
        }

        public async Task<CommentModel> ChangeComment(ChangeCommentModel newComment, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var comment = _context.Comments.Include(x => x.Author)
                                            .FirstOrDefault(x => x.Id == newComment.CommentId && x.IsActive);
            if (comment == null)
                throw new CommentNotFoundException();
            if (comment.Author.Id != userId)
                throw new UserDontHaveAccessException();
            comment.CommentText = newComment.CommentText;
            comment.Changed = true;
            await _context.SaveChangesAsync();
            return await GetComment(userId, comment.Id);
        }

        public async Task DeleteComment(Guid commentId, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var comment = _context.Comments.Include(x => x.Author)
                                           .Include(x => x.Post)
                                           .FirstOrDefault(x => x.Id == commentId && x.IsActive);
            if (comment == null)
                throw new CommentNotFoundException();
            if (comment.Author.Id != userId && comment.Post.AuthorID != userId)
                throw new UserDontHaveAccessException();
            comment.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostContent(Guid contentId, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var content = await _context.PostContent.Include(x => x.Author)
                                                    .Include(x => x.Post)
                                                    .FirstOrDefaultAsync(x => x.Id == contentId && x.IsActive);
            if (content == null)
                throw new ContentNotFoundException();
            if (content.Author.Id != userId)
                throw new UserDontHaveAccessException();
            content.IsActive = false;
            content.Post.Changed = true;
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

