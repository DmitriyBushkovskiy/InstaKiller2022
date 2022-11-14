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
            LinkGenerateHelper.LinkAvatarGenerator = x
                => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new { userId = x.Id });
            LinkGenerateHelper.LinkContentGenerator = x
                => Url.ControllerAction<AttachController>(nameof(AttachController.GetPostContent), new { postContentId = x.Id });
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
        [Route("{commentId}")]
        public async Task<CommentModel> GetComment(Guid commentId) => await _postService.GetComment(commentId);

        [HttpGet]
        [Route("{postId}")]
        public async Task<List<CommentModel>> GetComments(Guid postId) => await _postService.GetComments(postId);

        [HttpGet]
        [Route("{postId}")]
        public async Task<PostModel> GetPost(Guid postId) => await _postService.GetPost(postId);

        [HttpGet]
        public async Task<List<PostModel>> GetPosts(int skip, int take) => await _postService.GetPosts(skip, take);

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
