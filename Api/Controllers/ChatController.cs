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

        [HttpGet]
        [Route("{targetUserId}")]
        public async Task<string> GetIdOrCreatePrivateChat(Guid targetUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _chatService.GetIdOrCreatePrivateChat(userId, targetUserId);
        }

        [HttpPost]
        public async Task<string> CreateGroupChat(string chatName)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _chatService.CreateGroupChat(userId, chatName);
        }

        [HttpPut]
        public async Task AddUsersToGroupChat(RenewUsersInChatRequest model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            foreach (var id in model.TargetUsersId)
            {
                await _chatService.AddUserToGroupChat(userId, id, model.ChatId);
            }
        }

        [HttpPut]
        public async Task RenewGroupChatUsersList(RenewUsersInChatRequest model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            await _chatService.RenewGroupChatUsersList(userId, model);
        }

        [HttpPut]
        public async Task<List<MessageModel>> GetChat(ChatRequestModel model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _chatService.GetChat(userId, model);
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
        public async Task<ChatModel?> GetChatData(Guid chatId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new UserNotAuthorizedException();
            return await _chatService.GetChatData(userId, chatId);
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

        [HttpPut]
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
