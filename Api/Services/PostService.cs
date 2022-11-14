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

        public async Task<AttachModel> GetPostContent(Guid postContentId)
        {
            var res = await _context.PostContent.FirstOrDefaultAsync(x => x.Id == postContentId);
            return _mapper.Map<AttachModel>(res);
        }

        public async Task CreatePost(CreatePostRequest request, Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new Exception("user not found");  
            var post = new Post()
            {
                Id = Guid.NewGuid(),
                Author = user,
                AuthorID = userId,
                Created = DateTimeOffset.UtcNow,
                Description = request.Description,
            };
            post.Content = CreatePostContent(request.Contents, user, post);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        public List<PostContent> CreatePostContent(List<MetadataModel> models, User user, Post post)
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "attaches");
            var destFi = new FileInfo(directory);
            if (destFi.Directory != null && !destFi.Directory.Exists)
                destFi.Directory.Create();

            var result = models.Select(model =>
            {
                return _mapper.Map<MetadataModel, PostContent>(model, opts => opts.AfterMap((s, d) => 
                { 
                    d.PostID = post.Id;
                    d.Post = post;
                    d.Author = user;
                    d.FilePath = Path.Combine(directory, model.TempId.ToString());
                    var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), model.TempId.ToString()));
                    if (tempFi.Exists)
                        File.Move(tempFi.FullName, d.FilePath, true);
                }));
            }).ToList();
            return result;
        }

        public async Task AddContentToPost(List<MetadataModel> models, Guid userId, Guid postId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new Exception("user not found");
            if (!await CheckPostExist(postId))
                throw new Exception("post is not exist");
            if (!await CheckUserHasPost(userId, postId))
                throw new Exception("you are not the owner of the post");
            var post = await _context.Posts.FirstAsync(x => x.Id == postId);
            post.Content = CreatePostContent(models, user, post);
            post.Changed = true;
            await _context.SaveChangesAsync();
        }

        public async Task<PostModel> GetPost(Guid postId)
        {
            //TODO: проверка что можешь просматривать чужой пост
            if (!await CheckPostExist(postId))
                throw new Exception("post is not exist");
            var post = await _context.Posts.Include(x=>x.Author)
                                               .ThenInclude(x=>x.Avatar)
                                           .Include(x=>x.Comments)
                                           .Include(x=>x.Content)
                                           .FirstAsync(x => x.Id == postId);

            var postModel = _mapper.Map<PostModel>(post);
            postModel.PostContent = post.Content.Select(x=>_mapper.Map<PostContentModel>(x)).ToList();
            postModel.Comments = await GetComments(postId);
            return postModel;
        }

        public async Task CreateComment(CreateComment commentRequest, Guid userId)
        {
            if (!await CheckUserExist(userId))
                throw new Exception("user not found");
            if (!await CheckPostExist(commentRequest.PostId))
                throw new Exception("post is not exist");
            var comment = _mapper.Map<Comment>(commentRequest);
            comment.UserId = userId;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<CommentModel> GetComment(Guid commentId)
        {
            if (!await CheckCommentExist(commentId))
                throw new Exception("comment is not exist");
            var comment = await _context.Comments.FirstAsync(x => x.Id == commentId);
            var commentModel = _mapper.Map<CommentModel>(comment);
            commentModel.Username = _context.Users.First(x => x.Id == comment.UserId).Username;
            //TODO: добавить заполнение поля лайков для комментариев
            return commentModel;
        }

        public async Task<List<CommentModel>> GetComments(Guid postId)
        {
            if (!await CheckPostExist(postId))
                throw new Exception("post is not exist");
            var result = new List<CommentModel>();
            foreach (var commentId in _context.Comments.Where(x => x.PostId == postId).OrderBy(x => x.Created).Select(x => x.Id).ToList())
            {
                result.Add(await GetComment(commentId));
            }
            return result;
        }

        public async Task<List<PostModel>> GetPosts(int skip, int take)
        {
            var  Ids = _context.Posts
                .AsNoTracking()
                .OrderByDescending(x => x.Created)
                .Skip(skip)
                .Take(take)
                .Select(x => x.Id) 
                .ToList();
            var result = new List<PostModel>();
            foreach (var id in Ids)
            {
                result.Add(await GetPost(id));
            }
            return result;
        }

        //TODO: добавить метод изменения коммента
        //TODO: добавить метод изменения текста поста
        public async Task<bool> CheckPostExist(Guid postId)
        {
            return await _context.Posts.AnyAsync(x => x.Id == postId);
        }

        public async Task<bool> CheckUserExist(Guid userId)
        {
            return await _context.Users.AnyAsync(x => x.Id == userId);
        }

        public async Task<bool> CheckUserHasPost(Guid userId, Guid postId)
        {
            return await _context.Posts.Where(x => x.AuthorID == userId).AnyAsync(x => x.Id == postId);
        }

        public async Task<bool> CheckContentExist(Guid contentId)
        {
            return await _context.PostContent.AnyAsync(x => x.Id == contentId);
        }

        public async Task<bool> CheckCommentExist(Guid commentId)
        {
            return await _context.Comments.AnyAsync(x => x.Id == commentId);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
