using Api.Exceptions;
using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Post;
using Api.Models.PostContent;
using Api.Models.Relation;
using Api.Models.User;
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
    [ApiExplorerSettings(GroupName = "Api")]
    [Authorize]

    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly LikeService _likeService;

        public PostController(PostService postService, LikeService likeService, LinkGeneratorService linkGeneratorService)
        {
            _postService = postService;
            _likeService = likeService;
            linkGeneratorService.LinkAvatarGenerator = x
                => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new { userId = x.Id });
            linkGeneratorService.LinkContentGenerator = x
                => Url.ControllerAction<AttachController>(nameof(AttachController.GetPostContent), new { postContentId = x.Id });
        }

        // Posts
        [HttpPost]
        public async Task CreatePost(CreatePostRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _postService.CreatePost(request, userId);
        }

        [HttpGet]
        [Route("{postId}")]
        public async Task<PostModel> GetPost(Guid postId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _postService.GetPost(userId, postId);
        }

        [HttpGet]
        public async Task<List<PostModel>> GetPostsByUserId(Guid targetUserId, int skip = 0, int take = 10)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _postService.GetPostsByUserId(userId, targetUserId, skip, take);
        }

        [HttpGet]
        public async Task<List<PostModel>> GetPostFeed(int skip = 0, int take = 10)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _postService.GetPostFeed(userId, skip, take);
        } 

        [HttpPut]
        public async Task ChangePostDescription(ChangePostDescriptionModel model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _postService.ChangePostDescription(model, userId);
        }

        [HttpPut]
        [Route("{postId}")]
        public async Task<bool> LikePost(Guid postId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _likeService.LikePost(postId, userId);
        }

        [HttpDelete]
        [Route("{postId}")]
        public async Task DeletePost(Guid postId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _postService.DeletePost(postId, userId);
        }

        // Comments

        [HttpPost]
        public async Task CreateComment(CreateComment comment)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _postService.CreateComment(comment, userId);
        }

        [HttpGet]
        [Route("{commentId}")]
        public async Task<CommentModel> GetComment(Guid commentId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _postService.GetComment(userId, commentId);
        }

        [HttpGet]
        [Route("{postId}")]
        public async Task<List<CommentModel>> GetComments(Guid postId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _postService.GetComments(userId, postId);
        }

        [HttpPut]
        public async Task ChangeComment(ChangeCommentModel newComment)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _postService.ChangeComment(newComment, userId);
        }

        [HttpPut]
        [Route("{commentId}")]
        public async Task<bool> LikeComment(Guid commentId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _likeService.LikeComment(commentId, userId);
        }

        [HttpDelete]
        [Route("{commentId}")]
        public async Task DeleteComment(Guid commentId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _postService.DeleteComment(commentId, userId);
        }

        // Content

        [HttpPut]
        [Route("{contentId}")]
        public async Task<bool> LikeContent(Guid contentId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _likeService.LikeContent(contentId, userId);
        }

        [HttpDelete]
        [Route("{contentId}")]
        public async Task DeletePostContent(Guid contentId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _postService.DeletePostContent(contentId, userId);
        }
    }
}
