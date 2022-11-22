using Api.Models.Message;
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
    public class MessageController : ControllerBase
    {
        private readonly MessageService _messageService;

        public MessageController(MessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost]
        public async Task SendMessage(CreateMessageModel messageModel)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            await _messageService.SendMessage(messageModel, userId);
        }

        [HttpGet]
        public async Task<List<MessageModel>> GetChat(Guid targetUserId, int skip, int take )
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            return await _messageService.GetChat(userId, targetUserId, skip, take);
        }

        [HttpDelete]
        public async Task DeleteMessage(Guid messageId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
                throw new Exception("you are not authorized");
            await _messageService.DeleteMessage(userId, messageId);
        }
    }
}
