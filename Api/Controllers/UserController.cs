using Api.Exceptions;
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
    [ApiExplorerSettings(GroupName = "Api")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService, LinkGeneratorService linkGeneratorService)
        {
            _userService = userService;
            linkGeneratorService.LinkAvatarGenerator = x
                => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new { userId = x.Id });
        }

        [HttpPost]
        public async Task AddAvatarToUser(MetadataModel model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _userService.AddAvatarToUser(userId, model);
        }

        [HttpGet]
        public async Task<IEnumerable<UserWithAvatarLinkModel>> GetUsers() => await _userService.GetUsers();

        [HttpGet]
        public async Task<UserWithAvatarLinkModel> GetCurrentUser()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            else
                return await _userService.GetUser(userId);
        }

        [HttpGet]
        [Route("{targetUserId}")]
        public async Task<UserDataModel> GetUserData(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _userService.GetUserData(userId, targetUserId);
        }

        [HttpGet]
        public async Task<UserDataModel> GetCurrentUserData()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _userService.GetUserData(userId, userId);
        }

        [HttpPut]
        public async Task ChangeUserData(ChangeUserDataModel data)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _userService.ChangeUserData(data, userId);
        }

        [HttpPut]
        public async Task ChangeUsername(ChangeUsernameModel data)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _userService.ChangeUsername(data, userId);
        }

        [HttpPut]
        public async Task ChangeEmail(ChangeEmailModel data)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _userService.ChangeEmail(data, userId);
        }

        [HttpDelete]
        public async Task DeleteAccount()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _userService.DeleteUser(userId);
        }

        //TODO: добавить метод подтверждения почты 
    }
}
