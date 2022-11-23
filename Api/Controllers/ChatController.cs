using Api.Exceptions;
using Api.Models.Chat;
using Api.Models.Message;
using Api.Models.User;
using Api.Services;
using Common.Consts;
using Common.Extentions;
using DAL.Entities;
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
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService, LinkGeneratorService linkGeneratorService)
        {
            _chatService = chatService;
            linkGeneratorService.LinkAvatarGenerator = x
                => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new { userId = x.Id });
            linkGeneratorService.LinkContentGenerator = x
                => Url.ControllerAction<AttachController>(nameof(AttachController.GetPostContent), new { postContentId = x.Id });
        }

        //TODO: проверить эти методы еще раз

        [HttpPost]
        [Route("{targetUserId}")]
        public async Task<Guid> GetIdOrCreatePrivateChat(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _chatService.GetIdOrCreatePrivateChat(userId, targetUserId);
        }

        [HttpPost]
        public async Task<Guid> CreateGroupChat()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _chatService.CreateGroupChat(userId);
        }

        [HttpPut]
        public async Task AddUserToGroupChat(Guid targetUserId, Guid chatId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _chatService.AddUserToGroupChat(userId, targetUserId, chatId);
        }

        [HttpGet]
        public async Task<List<MessageModel>> GetChat(Guid chatId, int skip = 0, int take = 10)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _chatService.GetChat(userId, chatId, skip, take);
        }

        [HttpGet]
        public async Task<List<ChatModel>> GetChats(int skip = 0, int take = 10)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _chatService.GetChats(userId, skip, take);
        }

        [HttpGet]
        [Route("{chatId}")]
        public async Task<List<UserWithAvatarLinkModel>> GetChatParticipants(Guid chatId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _chatService.GetChatParticipants(chatId);
        }

        [HttpGet]
        public async Task<List<ChatModel>> GetChatRequests(int skip = 0, int take = 10)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _chatService.GetChatRequests(userId, skip, take);
        }

        [HttpPut]
        [Route("{chatId}")]
        public async Task AcceptRequest(Guid chatId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _chatService.AcceptRequest(userId, chatId);
        }

        [HttpPost]
        public async Task SendMessage(CreateMessageModel message)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _chatService.SendMessage(userId, message);
        }

        [HttpDelete]
        [Route("{messageId}")]
        public async Task DeleteMessage(Guid messageId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _chatService.DeleteMessage(userId, messageId);
        }
    }
}
