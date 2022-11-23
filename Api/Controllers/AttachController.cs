using Api.Exceptions;
using Api.Models.Attach;
using Api.Services;
using Common.Consts;
using Common.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Api")]
    [Authorize]
    public class AttachController : ControllerBase
    {
        private readonly AttachService _attachService;
        private readonly PostService _postService;
        private readonly UserService _userService;

        public AttachController(AttachService attachService, PostService postService, UserService userService)
        {
            _attachService = attachService;
            _postService = postService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<List<MetadataModel>> UploadFiles([FromForm] List<IFormFile> files)
            => await _attachService.UploadFiles(files);

        [HttpGet]
        [Route("{postContentId}")]
        public async Task<FileStreamResult> GetPostContent(Guid postContentId, bool download = false)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return RenderAttach(await _postService.GetPostContent(userId, postContentId), download);
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<FileStreamResult> GetUserAvatar(Guid userId, bool download = false)
            => RenderAttach(await _userService.GetUserAvatar(userId), download);

        [HttpGet]
        public async Task<FileStreamResult> GetCurrentUserAvatar(bool download = false)
            => RenderAttach(await _userService.GetUserAvatar(User.GetClaimValue<Guid>(ClaimNames.Id)), download);

        private FileStreamResult RenderAttach(AttachModel attach, bool download)
        {
            var fs = new FileStream(attach.FilePath, FileMode.Open);
            var ext = Path.GetExtension(attach.Name);
            if (download)
                return File(fs, attach.MimeType, $"{attach.Id}{ext}");
            else
                return File(fs, attach.MimeType);
        }
    }
}
