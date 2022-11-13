using Api.Models.Attach;
using Api.Models.User;
using Api.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using Common.Consts;
using Common.Extentions;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
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
        public async Task AddAvatarToUser(MetadataModel model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), model.TempId.ToString()));
                if (!tempFi.Exists)
                    throw new Exception("file not found");
                else
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "attaches", model.TempId.ToString());
                    var destFi = new FileInfo(path);
                    if (destFi.Directory != null && !destFi.Directory.Exists)
                        destFi.Directory.Create();
                    System.IO.File.Copy(tempFi.FullName, path, true);
                    await _userService.AddAvatarToUser(userId, model, path);
                }
            }
            else
                throw new Exception("you are not authorized");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<FileStreamResult> GetUserAvatar(Guid userId, bool download = false)
        {
            var attach = await _userService.GetUserAvatar(userId);
            var fs = new FileStream(attach.FilePath, FileMode.Open);
            if (download)
                return File(fs, attach.MimeType, attach.Name);
            else
                return File(fs, attach.MimeType);
        }

        [HttpGet]
        public async Task<FileStreamResult> GetCurrentUserAvatar(bool dowhload = false)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await GetUserAvatar(userId, dowhload);
            }
            else
                throw new Exception("you are not authorized");
        }

        [HttpGet]
        public async Task<IEnumerable<UserWithAvatarLinkModel>> GetUsers() => await _userService.GetUsers();

        [HttpGet]
        public async Task<UserWithAvatarLinkModel> GetCurrentUser()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _userService.GetUser(userId);
            }
            else
                throw new Exception("you are not authorized");
        }
        //TODO: добавить методы:
        // изменения информации о пользователе
        // подтверждения почты 
        // изменения почты
        // подписки/отписки
        // получить список подписчиков
        // получить список на кого подписан
    }
}
