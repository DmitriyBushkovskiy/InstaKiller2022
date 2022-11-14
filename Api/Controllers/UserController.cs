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
            LinkGenerateHelper.LinkAvatarGenerator = x
                => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new { userId = x.Id });
        }

        [HttpPost]
        public async Task AddAvatarToUser(MetadataModel model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id); // TODO: написать покороче c использованием афтермапа
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
                    System.IO.File.Move(tempFi.FullName, path, true);
                    await _userService.AddAvatarToUser(userId, model, path);
                }
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
            if (userId == default)
            {
                throw new Exception("you are not authorized");
            }
            else
                return await _userService.GetUser(userId);
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<UserDataModel> GetUserData(Guid userId) => await _userService.GetUserData(userId);

        [HttpGet]
        public async Task<UserDataModel> GetCurrentUserData()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
            {
                throw new Exception("you are not autorized");
            }
            else
                return await GetUserData(userId);
        }

        [HttpPut]
        [Route("{userId}")]
        public async Task ChangeUserData(ChangeUserDataModel data, Guid userId) => await _userService.ChangeUserData(data, userId);

        [HttpPut]
        public async Task ChangeCurrentUserData(ChangeUserDataModel data)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
            {
                throw new Exception("you are not autorized");
            }
            else
                await ChangeUserData(data, userId);
        }

        [HttpPut]
        [Route("{userId}")]
        public async Task ChangeUsername(ChangeUsernameModel data, Guid userId) => await _userService.ChangeUsername(data, userId);

        [HttpPut]
        public async Task ChangeCurrentUsername(ChangeUsernameModel data)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
            {
                throw new Exception("you are not autorized");
            }
            else
                await ChangeUsername(data, userId);
        }

        [HttpPut]
        [Route("{userId}")]
        public async Task ChangeEmail(ChangeEmailModel data, Guid userId) => await _userService.ChangeEmail(data, userId);

        [HttpPut]
        public async Task ChangeCurrentEmail(ChangeEmailModel data)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
            {
                throw new Exception("you are not autorized");
            }
            else
                await ChangeEmail(data, userId);
        }
        //TODO: добавить методы:
        // подтверждения почты 
        // изменения почты
        // подписки/отписки
        // получить список подписчиков
        // получить список на кого подписан
    }
}
