using Api.Models.Relation;
using Api.Services;
using Common.Consts;
using Common.Extentions;
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

        public RelationController(RelationService relationService)
        {
            _relationService = relationService;
        }

        /// <summary>
        /// State = true - подписан
        /// State = null - подал заявку на подписку (у закрытых аккаунтов)
        /// State = false - забаненый аккаунт
        /// </summary>
        
        [HttpGet]
        [Route("{targetUserId}")]
        public async Task<List<FollowedModel>> GetFollowed(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            return await _relationService.GetFollowed(userId, targetUserId);
        }

        [HttpGet]
        public async Task<List<FollowerModel>> GetFollowersRequests()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            return await _relationService.GetFollowersRequests(userId);
        }

        [HttpGet]
        public async Task<List<FollowerModel>> GetBanned()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            return await _relationService.GetBanned(userId);
        }

        [HttpGet]
        [Route("{targetUserId}")]
        public async Task<List<FollowerModel>> GetFollowers(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            return await _relationService.GetFollowers(userId, targetUserId);
        }
        
        [HttpPut]
        [Route("{targetUserId}")]
        public async Task<bool?> Follow(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            return await _relationService.Follow(userId, targetUserId);
        }

        [HttpPut]
        [Route("{targetUserId}")]
        public async Task Ban(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            await _relationService.Ban(userId, targetUserId);
        }

        [HttpPut]
        [Route("{targetUserId}")]
        public async Task Unban(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            await _relationService.Unban(userId, targetUserId);
        }
    }
}
