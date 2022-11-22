using Api.Configs;
using Api.Models.Comment;
using AutoMapper;
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

        public async Task<bool> LikePost(Guid postId, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new Exception("user not found");
            var post = await _context.Posts.AsNoTracking()
                                            .Include(x => x.Author)
                                               .ThenInclude(x => x.Followers.Where(y => y.FollowerId == userId))
                                            .Include(x => x.Likes)
                                            .FirstOrDefaultAsync(x => x.Id == postId && x.IsActive);
            if (post == null)
                throw new Exception("post not found");
            if (!post.Author.PrivateAccount && post.Author.Followers.FirstOrDefault()?.State != false
                || post.Author.Followers.FirstOrDefault()?.State == true
                || userId == post.AuthorID)
            {
                var like = post.Likes.FirstOrDefault(x => x.UserId == userId);
                if (like == null)
                {
                    like = new PostLike()
                    {
                        PostId = postId,
                        UserId = userId,
                    };
                    _context.PostLikes.Add(like);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    _context.PostLikes.Remove(like);
                    await _context.SaveChangesAsync();
                    return false;
                }
            }
            throw new Exception("you don't have access");
        }

        public async Task<bool> LikeComment(Guid commentId, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new Exception("user not found");
            var comment = await _context.Comments.AsNoTracking()
                                                    .Include(x => x.Likes)
                                                    .Include(x => x.Post)
                                                        .ThenInclude(x => x.Author)
                                                            .ThenInclude(x => x.Followers.Where(y => y.FollowerId == userId))
                                                    .FirstOrDefaultAsync(x => x.Id == commentId && x.IsActive);
            if (comment == null)
                throw new Exception("comment not found");
            if (!comment.Post.Author.PrivateAccount && comment.Post.Author.Followers.FirstOrDefault()?.State != false
                || comment.Post.Author.Followers.FirstOrDefault()?.State == true
                || userId == comment.UserId)
            {
                var like = comment.Likes.FirstOrDefault(x => x.UserId == userId);
                if (like == null)
                {
                    like = new CommentLike()
                    {
                        CommentId = commentId,
                        UserId = userId
                    };
                    _context.CommentLikes.Add(like);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    _context.CommentLikes.Remove(like);
                    await _context.SaveChangesAsync();
                    return false;
                }
            }
            else
                throw new Exception("you don't have access");
        }

        public async Task<bool> LikeContent(Guid contentId, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new Exception("user not found");

            var content = await _context.PostContent.Include(x => x.Likes)
                                                     .Include(x => x.Post)
                                                        .ThenInclude(x => x.Author)
                                                            .ThenInclude(x => x.Followers.Where(y => y.FollowerId == userId))
                                                    .FirstOrDefaultAsync(x => x.Id == contentId && x.IsActive == true);
            if (content == null)
                throw new Exception("content not found");
            if (!content.Post.Author.PrivateAccount && content.Post.Author.Followers.FirstOrDefault()?.State != false
                || content.Post.Author.Followers.FirstOrDefault()?.State == true
                || userId == content.AuthorId)
            {
                var like = content.Likes.FirstOrDefault(x => x.UserId == userId);
                if (like == null)
                {
                    like = new ContentLike()
                    {
                        ContentId = contentId,
                        UserId = userId
                    };
                    _context.ContentLikes.Add(like);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    _context.ContentLikes.Remove(like);
                    await _context.SaveChangesAsync();
                    return false;
                }
            }
            else
                throw new Exception("you don't have access");
        }
    }
}
