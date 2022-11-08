using Api.Models;
using Api.Services;
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
        private readonly UserService _userService;

        public PostController (PostService postService, UserService userService)
        {
            _postService = postService;
            _userService = userService;
        }

        [HttpPost]
        public async Task CreatePostWithUploadingFiles([FromForm] List<IFormFile> files, string postText)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            var userId = _postService.ParseStringToGuid(userIdString!);
            await _postService.CreatePostWithUploadingFiles(files, userId, postText);
        }

        [HttpPost]
        public async Task CreatePost(List<MetadataModel> models, string postText)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            var userId = _postService.ParseStringToGuid(userIdString!);
            await _postService.CreatePost(models, userId, postText);
        }

        [HttpPost]
        public async Task AddContentToPost(List<MetadataModel> models, Guid postId)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            var userId = _postService.ParseStringToGuid(userIdString!);
            await _postService.AddContentToPost(models, userId, postId);
        }

        [HttpPost]
        public async Task CreateComment (string commentText, Guid postId)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            var userId = _postService.ParseStringToGuid(userIdString!);
            await _postService.CreateComment(commentText, postId, userId);
        }

        [HttpGet]
        public async Task<CommentModel> GetCommentById(Guid commentId) => await _postService.GetCommentById(commentId);

        [HttpGet]
        public async Task<List<CommentModel>> GetCommentsByPostId(Guid postId) => await _postService.GetCommentsByPostId(postId);

        [HttpGet]
        public async Task<List<PostContentModel>> GetContentByPostId(Guid postId) => await _postService.GetContentByPostId(postId);

        [HttpGet]
        public async Task<FileResult> GetAttachById(Guid attachId)
        {
            var attach = await _postService.GetAttachById(attachId);
            return File(System.IO.File.ReadAllBytes(attach.FilePath), attach.MimeType);
        }

        [HttpGet]
        public async Task<PostModel> GetPostByID(Guid postId) => await _postService.GetPostModelByID(postId);


        //TODO: добавить методы:
        // лайк поста
        // лайк коммента
        // изменения поста
        // удаления поста
        // изменения коммента
        // удаления коммента
        // получение постов пользователя
    }
}
