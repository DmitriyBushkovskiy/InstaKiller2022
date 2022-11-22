using Api.Models.Message;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class MessageService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public MessageService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task SendMessage(CreateMessageModel messageModel, Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new Exception("user not found");
            var recipient = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == userId))
                                                 .FirstOrDefaultAsync(x => x.Id == messageModel.RecipientId && x.IsActive);
            if (recipient == default)
                throw new Exception("recipient not found");
            if (!recipient.PrivateAccount && recipient.Followers.FirstOrDefault()?.State != false
            || recipient.Followers.FirstOrDefault()?.State == true
            || userId == recipient.Id)
            {
                var message = _mapper.Map<Message>(messageModel);
                message.AuthorId = userId;
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
            }
            else
                throw new Exception("you don't have access");
        }

        public async Task<List<MessageModel>> GetChat(Guid userId, Guid targetUserId, int skip, int take)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new Exception("user not found");
            var targetUser = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == userId))
                                     .FirstOrDefaultAsync(x => x.Id == targetUserId && x.IsActive);
            if (targetUser == default)
                throw new Exception("recipient not found");
            if (!targetUser.PrivateAccount && targetUser.Followers.FirstOrDefault()?.State != false
            || targetUser.Followers.FirstOrDefault()?.State == true
            || userId == targetUserId)
            {
                var result = new List<MessageModel>();
                await _context.Messages.Where(x => x.IsActive && (x.AuthorId == userId && x.RecipientId == targetUserId || x.AuthorId == targetUserId && x.RecipientId == userId))
                    .OrderByDescending(x => x.Created)
                    .Skip(skip)
                    .Take(take)
                    .ForEachAsync(x => 
                    {
                        if (x.AuthorId == targetUserId && x.RecipientId == userId && !x.State)
                            x.State = true;
                        result.Add(_mapper.Map<MessageModel>(x));
                    });
                await _context.SaveChangesAsync();
                return result;
            }
            else
                throw new Exception("you don't have access");
        }

        public async Task DeleteMessage(Guid userId, Guid messageId)
        {
            var message = await _context.Messages.Include(x => x.Author)
                                                 .FirstOrDefaultAsync(x => x.Id == messageId && x.IsActive);
            if (message == null)
                throw new Exception("message not found");
            if (message.Author.Id == userId)
            { 
                message.IsActive = false;
                await _context.SaveChangesAsync();
            }
            else
                throw new Exception("this is not your message");
        }

    }
}
