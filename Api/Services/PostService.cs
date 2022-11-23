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

namespace Api.Services
{
    public class PostService : IDisposable
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

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
            if (!content.Author.PrivateAccount && content.Author.Followers.FirstOrDefault()?.State != false
            || content.Author.Followers.FirstOrDefault()?.State == true
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
                                           .Include(x => x.Content.Where(y => y.IsActive))
                                               .ThenInclude(x => x.Likes)
                                           .Include(x => x.Likes)
                                           .FirstOrDefaultAsync(x => x.Id == postId && x.IsActive);
            if (post == null)
                throw new PostNotFoundException();
            if (!post.Author.PrivateAccount && post.Author.Followers.FirstOrDefault()?.State != false
                || post.Author.Followers.FirstOrDefault()?.State == true
                || userId == post.AuthorID)
                return _mapper.Map<PostModel>(post);
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
            if (!targetUser.PrivateAccount && targetUser.Followers.FirstOrDefault()?.State != false
                || targetUser.Followers.FirstOrDefault()?.State == true
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
                                                            .Where(x => x.FollowerId == userId && x.Followed.IsActive && x.State == true)
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

        public async Task CreateComment(CreateComment commentRequest, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var post = await _context.Posts.Include(x => x.Author)
                                                .ThenInclude(x => x.Followers.Where(y => y.FollowerId == userId))
                                            .FirstOrDefaultAsync(x => x.Id == commentRequest.PostId && x.IsActive);
            if (post == null)
                throw new PostNotFoundException();
            if (!post.Author.PrivateAccount && post.Author.Followers.FirstOrDefault()?.State != false
                || post.Author.Followers.FirstOrDefault()?.State == true
                || userId == post.AuthorID)
            {
                var comment = _mapper.Map<Comment>(commentRequest);
                comment.UserId = userId;
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
                                                 .Include(x => x.Post)
                                                    .ThenInclude(x => x.Author)
                                                        .ThenInclude(x => x.Followers.Where(y => y.FollowerId == userId))
                                                 .FirstOrDefaultAsync(x => x.Id == commentId && x.IsActive);
            if (comment == null)
                throw new CommentNotFoundException();
            if (!comment.Post.Author.PrivateAccount && comment.Post.Author.Followers.FirstOrDefault()?.State != false
                || comment.Post.Author.Followers.FirstOrDefault()?.State == true
                || userId == comment.UserId)
                return _mapper.Map<CommentModel>(comment);
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
                                            .FirstOrDefaultAsync(x => x.Id == postId && x.IsActive);
            if (post == null)
                throw new PostNotFoundException();
            if (!post.Author.PrivateAccount && post.Author.Followers.FirstOrDefault()?.State != false
                || post.Author.Followers.FirstOrDefault()?.State == true
                || userId == post.AuthorID)
                return post.Comments.OrderByDescending(x => x.Created).Select(x => _mapper.Map<CommentModel>(x)).ToList();
            else
                throw new UserDontHaveAccessException();
        }

        public async Task ChangeComment(ChangeCommentModel newComment, Guid userId)
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
