using Api.Configs;
using Api.Models;
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

namespace Api.Services
{
    public class PostService : IDisposable
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly AuthConfig _config;
        private readonly AttachService _attachService;

        public PostService(IMapper mapper, DataContext context, IOptions<AuthConfig> config)
        {
            _mapper = mapper;
            _context = context;
            _config = config.Value;
            _attachService = new AttachService(mapper, context, config); // TODO: переделать
        }

        public async Task CreatePostWithUploadingFiles(List<IFormFile> files, Guid userId, string postText)
        {
            var models = await _attachService.UploadFiles(files);
            await CreatePost(models, userId, postText);
        }

        public async Task CreatePost(List<MetadataModel> models, Guid userId, string postText)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new Exception("user not found");
            }
            var post = new Post { AuthorID = userId, Created = DateTimeOffset.UtcNow, Id = Guid.NewGuid(), Text = postText };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            post.Content = await CreatePostContent(models, user, post.Id);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PostContent>> CreatePostContent(List<MetadataModel> models, User user, Guid postID)
        {
            var result = new List<PostContent>();
            foreach (var model in models)
            {
                var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), model.TempId.ToString()));
                if (!tempFi.Exists)
                    throw new Exception("file not found");
                var path = Path.Combine(Directory.GetCurrentDirectory(), "attaches", model.TempId.ToString());
                var destFi = new FileInfo(path);
                if (destFi.Directory != null && !destFi.Directory.Exists)
                    destFi.Directory.Create();
                File.Copy(tempFi.FullName, path, true);
                var postImage = new PostContent() {   Author = user, 
                                                    FilePath = path, 
                                                    MimeType = model.MimeType, 
                                                    Name = model.Name, 
                                                    Size = model.Size, 
                                                    PostID = postID, 
                                                    User = user,
                                                    UserID = user.Id };
                result.Add(postImage);
                _context.PostContent.Add(postImage);
            }
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task AddContentToPost(List<MetadataModel> models, Guid userId, Guid postId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new Exception("user not found");
            }
            if (!await CheckPostExist(postId))
            {
                throw new Exception("post is not exist");
            }
            if (!await CheckUserHasPost(userId, postId))
            {
                throw new Exception("you are not the owner of the post");
            };
            var post = await GetPostById(postId);
            post.Content = await CreatePostContent(models, user, postId);
            await _context.SaveChangesAsync();
        }

        public async Task< List<Guid>> GetListPostsIdsByUserId(Guid userId)
        {
            if (!await CheckUserExist(userId))
            {
                throw new Exception("user not found");
            }
            return await _context.Posts.Where(x => x.AuthorID == userId).Select(x => x.Id).ToListAsync();
        }

        private async Task<Post> GetPostById(Guid postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
            {
                throw new Exception("post is not exist");
            }
            else 
            {
                return post; 
            }
        }

        public async Task<PostModel> GetPostModelByID(Guid postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
            {
                throw new Exception("post is not exist");
            }
            var postModel = _mapper.Map<PostModel>(post);
            postModel.PostContent = await GetContentByPostId(postId);
            postModel.Comments = await GetCommentsByPostId(postId);
            postModel.Author = await GetUserAvatarLinkById(post.AuthorID);
            return postModel;
        }
        private async Task<UserAvatarModel> GetUserAvatarLinkById(Guid id)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                throw new Exception("user not found");
            var userAvatar = _mapper.Map<UserAvatarModel>(user) ;
            userAvatar.Avarar = user.Avatar == null ? null : GetLinkToAttachById(user.Avatar.Id);
            return userAvatar;
        }

        public async Task CreateComment(string commentText, Guid postId, Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new Exception("user not found");
            }
            if (!await CheckPostExist(postId))
            {
                throw new Exception("post is not exist");
            }
            var post = await GetPostById(postId);
            var comment = new Comment() {   Id = Guid.NewGuid(), 
                                            CommentText = commentText, 
                                            Created = DateTimeOffset.UtcNow, 
                                            PostId = postId, 
                                            UserId = userId, 
                                            Changed = false, 
                                            Author = user, 
                                            Post = post };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<CommentModel> GetCommentById(Guid commentId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null)
            {
                throw new Exception("comment is not exist");
            }
            var commentModel = _mapper.Map<CommentModel>(comment);
            var username = _context.Users.FirstOrDefault(x => x.Id == comment.UserId)?.Username;
            if (username == null)
            {
                throw new Exception("username field is empty");
            }
            commentModel.Username = username;
            //TODO: добавить заполнение поля лайков
            return commentModel;
        }

        public async Task<List<CommentModel>> GetCommentsByPostId(Guid postId)
        {
            if (!await CheckPostExist(postId))
            {
                throw new Exception("post is not exist");
            }
            var result = new List<CommentModel>();
            var commentsIds = _context.Comments.Where(x => x.PostId == postId).OrderBy(x=>x.Created).Select(x => x.Id).ToList();
            foreach (var commentId in commentsIds)
            {
                var comment = await GetCommentById(commentId);
                result.Add(comment);
            }
            return result;
        }

        public async Task<PostContentModel> GetPostContentById(Guid poctContentId)
        {
            var attach = await _context.Attaches.FirstOrDefaultAsync(x => x.Id == poctContentId);
            return _mapper.Map<PostContentModel>(attach);
        }

        public async Task<List<PostContentModel>> GetContentByPostId(Guid postId)
        {
            var result = new List<PostContentModel>();
            var IDs = await GetListContentIdsByPostId(postId);
            foreach (var postImageId in IDs)
            {
                var content = await GetPostContentById(postImageId);
                content.connectLink = GetLinkToAttachById(postImageId);
                result.Add(content);
            }
            return result;
        }

        [AllowAnonymous]
        public async Task<AttachModel> GetAttachById(Guid attachId)
        {
            if (!await CheckAttachExist(attachId))
            {
                throw new Exception("Image not exist");
            }
            var postImage = await _context.Attaches.FirstOrDefaultAsync(x => x.Id == attachId);
            var attach = _mapper.Map<AttachModel>(postImage);
            return attach;
        }

        public async Task<List<Guid>> GetListContentIdsByPostId(Guid postId)
        {
            return await _context.PostContent.Where(x => x.PostID == postId).Select(x=>x.Id).ToListAsync();
        }

        public string GetLinkToAttachById(Guid attachId)
        {
            return @"/api/Post/" + nameof(GetAttachById) + "?attachId=" + attachId.ToString();
        }

        public Guid ParseStringToGuid(string _string)
        {
            if (Guid.TryParse(_string, out var guid))
            {
                return guid;
            }
            else
                throw new Exception("you are not authorized");
        }

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
            var userPosts = await GetListPostsIdsByUserId(userId);
            return userPosts.Any(x => x == postId);
        }

        public async Task<bool> CheckAttachExist(Guid postImageId)
        {
            return await _context.Attaches.AnyAsync(x => x.Id == postImageId);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
