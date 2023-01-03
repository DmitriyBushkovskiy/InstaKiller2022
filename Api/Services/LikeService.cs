using Api.Configs;
using Api.Exceptions;
using Api.Models.Comment;
using Api.Models.Like;
using AutoMapper;
using Common.Enums;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Numerics;

namespace Api.Services
{
    public class LikeService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly AuthConfig _config;

        public LikeService(IMapper mapper, IOptions<AuthConfig> config, DataContext context)
        {
            _mapper = mapper;
            _context = context;
            _config = config.Value;
        }

        public async Task<LikeDataModel> LikePost(Guid postId, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var post = await _context.Posts.AsNoTracking()
                                            .Include(x => x.Author)
                                               .ThenInclude(x => x.Followers.Where(y => y.FollowerId == userId))
                                            .Include(x => x.Likes)
                                            .FirstOrDefaultAsync(x => x.Id == postId && x.IsActive);
            if (post == null)
                throw new PostNotFoundException();
            LikeDataModel likeDataModel;
            if (!post.Author.PrivateAccount && post.Author.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
                || post.Author.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
                || userId == post.AuthorID)
            {
                var like = post.Likes.FirstOrDefault(x => x.UserId == userId);
                if (like == null)
                {
                    like = new PostLike()
                    {
                        PostId = postId,
                        UserId = userId,
                        Created = DateTimeOffset.UtcNow,
                    };
                    likeDataModel = new LikeDataModel() { LikedByMe = true, LikesAmount = post.Likes.Count + 1 };
                    _context.PostLikes.Add(like);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    likeDataModel = new LikeDataModel() { LikedByMe = false, LikesAmount = post.Likes.Count - 1 };
                    _context.PostLikes.Remove(like);
                    await _context.SaveChangesAsync();
                }
                return likeDataModel;
            }
            throw new UserDontHaveAccessException();
        }

        public async Task<LikeDataModel> LikeComment(Guid commentId, Guid userId)
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
            LikeDataModel likeDataModel;
            if (!comment.Post.Author.PrivateAccount && comment.Post.Author.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
                || comment.Post.Author.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
                || userId == comment.AuthorId)
            {
                var like = comment.Likes.FirstOrDefault(x => x.UserId == userId);
                if (like == null)
                {
                    like = new CommentLike()
                    {
                        CommentId = commentId,
                        UserId = userId,
                        Created = DateTimeOffset.UtcNow,
                    };
                    likeDataModel = new LikeDataModel() { LikedByMe = true, LikesAmount = comment.Likes.Count + 1 };
                    _context.CommentLikes.Add(like);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    likeDataModel = new LikeDataModel() { LikedByMe = false, LikesAmount = comment.Likes.Count - 1 };
                    _context.CommentLikes.Remove(like);
                    await _context.SaveChangesAsync();
                }
                return likeDataModel;
            }
            else
                throw new UserDontHaveAccessException();
        }

        public async Task<LikeDataModel> LikeContent(Guid contentId, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();

            var content = await _context.PostContent.Include(x => x.Likes)
                                                     .Include(x => x.Post)
                                                        .ThenInclude(x => x.Author)
                                                            .ThenInclude(x => x.Followers.Where(y => y.FollowerId == userId))
                                                    .FirstOrDefaultAsync(x => x.Id == contentId && x.IsActive == true);
            if (content == null)
                throw new ContentNotFoundException();
            LikeDataModel likeDataModel;
            if (!content.Post.Author.PrivateAccount && content.Post.Author.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
                || content.Post.Author.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
                || userId == content.AuthorId)
            {
                var like = content.Likes.FirstOrDefault(x => x.UserId == userId);
                if (like == null)
                {
                    like = new ContentLike()
                    {
                        ContentId = contentId,
                        UserId = userId,
                        Created = DateTimeOffset.UtcNow,
                    };
                    likeDataModel = new LikeDataModel() { LikedByMe = true, LikesAmount = content.Likes.Count + 1 };
                    _context.ContentLikes.Add(like);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    likeDataModel = new LikeDataModel() { LikedByMe = false, LikesAmount = content.Likes.Count - 1 };
                    _context.ContentLikes.Remove(like);
                    await _context.SaveChangesAsync();
                }
                return likeDataModel;
            }
            else
                throw new UserDontHaveAccessException();
        }
    }
}
