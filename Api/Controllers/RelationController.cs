using Api.Exceptions;
using Api.Models.Relation;
using Api.Models.User;
using Api.Services;
using Common.Consts;
using Common.Extentions;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Api")]
    [Authorize]
    public class RelationController : ControllerBase
    {
        private readonly RelationService _relationService;

        public RelationController(RelationService relationService, LinkGeneratorService linkGeneratorService)
        {
            linkGeneratorService.LinkAvatarGenerator = x
                => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new { userId = x.Id });
            _relationService = relationService;
            _relationService.UserId = x => User.GetClaimValue<Guid>(ClaimNames.Id);
        }

        /// <summary>
        /// State = true - подписан
        /// State = null - подал заявку на подписку (у закрытых аккаунтов)
        /// State = false - забаненый аккаунт
        /// </summary>
        
        [HttpPut]
        public async Task<List<UserWithAvatarLinkModel>> GetFollowed(DataByUserIdRequest model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _relationService.GetFollowed(userId, model);
        }

        [HttpPut]
        public async Task<List<UserWithAvatarLinkModel>> GetFollowersRequests(DataByUserIdRequest model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _relationService.GetFollowersRequests(userId, model);
        }

        [HttpPut]
        public async Task<List<UserWithAvatarLinkModel>> GetBanned(DataByUserIdRequest model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _relationService.GetBanned(userId, model);
        }

        [HttpPut]
        public async Task<List<UserWithAvatarLinkModel>> GetFollowers(DataByUserIdRequest model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _relationService.GetFollowers(userId, model);
        }

        [HttpGet]
        [Route("{targetUserId}")]
        public async Task<string> GetMyRelationState(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _relationService.GetMyRelationState(userId, targetUserId);
        }

        [HttpGet]
        [Route("{targetUserId}")]
        public async Task<RelationStateModel?> GetRelations(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _relationService.GetRelations(userId, targetUserId);
        }

        [HttpPut]
        public async Task<List<UserWithAvatarLinkModel>> SearchUsers(SearchUsersRequestModel model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _relationService.SearchUsers(userId, model);
        }

        [HttpGet]
        [Route("{targetUserId}")]
        public async Task<string> GetRelationToMeState(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _relationService.GetRelationToMeState(userId, targetUserId);
        }

        [HttpPut]
        [Route("{targetUserId}")]
        public async Task<string> Follow(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _relationService.Follow(userId, targetUserId);
        }

        [HttpPut]
        [Route("{targetUserId}")]
        public async Task<string> Ban(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _relationService.Ban(userId, targetUserId);
        }

        [HttpPut]
        [Route("{targetUserId}")]
        public async Task<string> Unban(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _relationService.Unban(userId, targetUserId);
        }

        [HttpPut]
        [Route("{targetUserId}")]
        public async Task<string> AcceptRequest(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _relationService.AcceptRequest(userId, targetUserId);
        }
    }
}
