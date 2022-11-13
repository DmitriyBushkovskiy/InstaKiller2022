using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Post;
using Api.Models.PostContent;
using Api.Services;
using Common;
using Common.Consts;
using Common.Extentions;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;

        public PostController (PostService postService, UserService userService)
        {
            _postService = postService;
            LinkGenerateHelper.LinkAvatarGenerator = x => Url.Action(nameof(UserController.GetUserAvatar), "User", new //TODO: дублирование с пост контроллером
            {
                userId = x.Id,
                download = false
            });
            LinkGenerateHelper.LinkContentGenerator = x => Url.Action(nameof(PostController.GetPostContent), "Post", new //TODO: дублирование с пост контроллером
            {
                postContentId = x.Id,
                download = false
            });
        }

        [HttpPost]
        public async Task CreatePost(CreatePostRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            await _postService.CreatePost(request, userId);
        }

        [HttpPost]
        public async Task AddContentToPost(List<MetadataModel> models, Guid postId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            await _postService.AddContentToPost(models, userId, postId);
        }

        //TODO: добавить метод "изменить пост" (текст поста)

        [HttpPost]
        public async Task CreateComment (CreateComment comment)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            await _postService.CreateComment(comment, userId);
        }

        [HttpGet]
        public async Task<CommentModel> GetComment(Guid commentId) => await _postService.GetComment(commentId);

        [HttpGet]
        public async Task<List<CommentModel>> GetComments(Guid postId) => await _postService.GetComments(postId);

        [HttpGet]
        [AllowAnonymous]
        public async Task<FileStreamResult> GetPostContent(Guid postContentId, bool download = false)
        {
            var attach = await _postService.GetPostContent(postContentId);
            var fs = new FileStream(attach.FilePath, FileMode.Open);
            if (download)
                return File(fs, attach.MimeType, attach.Name);
            else
                return File(fs, attach.MimeType);
        }

        [HttpGet]
        public async Task<PostModel> GetPost(Guid postId) => await _postService.GetPost(postId);

        //TODO: добавить методы:
        // лайк поста
        // лайк коммента
        // лайк контента
        // изменения поста
        // удаления поста
        // изменения коммента
        // удаления коммента
        // получение постов пользователя
    }
}
