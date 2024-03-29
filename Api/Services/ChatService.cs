﻿using Api.Exceptions;
using Api.Models.Chat;
using Api.Models.Message;
using Api.Models.User;
using AutoMapper;
using Common.Enums;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Api.Services
{
    public class ChatService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public ChatService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<string> GetIdOrCreatePrivateChat(Guid userId, Guid targetUserId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var targetUser = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == userId))
                                                 .FirstOrDefaultAsync(x => x.Id == targetUserId && x.IsActive);
            if (targetUser == default)
                throw new UserNotFoundException();
            var guids = new List<Guid> { userId, targetUserId };
            var chat = await _context.Chats.FirstOrDefaultAsync(x => x.IsPrivate && x.Participants.Count == 2 && x.Participants.All(y => guids.Contains(y.UserId)));
            if (chat == null)
            {
                if (!targetUser.PrivateAccount && targetUser.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
                    || targetUser.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
                    || userId == targetUser.Id)
                {
                    chat = new Chat()
                    {
                        IsPrivate = true,
                        CreatorId = userId,
                        Created = DateTimeOffset.UtcNow
                    };
                    chat.Participants.Add(new ChatParticipant() { UserId = userId, State = true });
                    chat.Participants.Add(new ChatParticipant() { UserId = targetUserId, State = true });
                    var chatEntity = await _context.Chats.AddAsync(chat);
                    await _context.SaveChangesAsync();
                    return chatEntity.Entity.Id.ToString();
                }
                else
                    throw new UserDontHaveAccessException();
            }
            else
                return chat.Id.ToString();
        }

        public async Task<string> CreateGroupChat(Guid userId, string chatName)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var chat = new Chat()
            {
                IsPrivate = false,
                CreatorId = userId,
                Created = DateTimeOffset.UtcNow,
                Name = chatName,
            };
            chat.Participants.Add(new ChatParticipant() { UserId = userId, State = true });
            var chatEntity = await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();
            return chatEntity.Entity.Id.ToString();
        }

        public async Task AddUserToGroupChat(Guid userId, Guid targetUserId, Guid chatId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var chat = await _context.Chats.FirstOrDefaultAsync(x => !x.IsPrivate && x.Id == chatId && x.Participants.Any(y => y.UserId == userId));
            if (chat == null)
                throw new ChatNotFoundException();
            var targetUser = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == userId))
                                                 .FirstOrDefaultAsync(x => x.Id == targetUserId && x.IsActive);
            if (targetUser == default)
                throw new UserNotFoundException();
            if (!targetUser.PrivateAccount && targetUser.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
            || targetUser.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
            || userId == targetUserId)
            {
                chat.Participants.Add(new ChatParticipant()
                {
                    State = true, //TODO: or null?
                    UserId = targetUserId
                });
                await _context.SaveChangesAsync();
            }
            else
                throw new UserDontHaveAccessException();
        }

        public async Task RenewGroupChatUsersList(Guid userId, RenewUsersInChatRequest model)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var chat = await _context.Chats.Include(x => x.Participants).FirstOrDefaultAsync(x => !x.IsPrivate && x.Id == model.ChatId && x.CreatorId == userId);
            if (chat == null)
                throw new ChatNotFoundException();
            var users = await _context.Users
                                            .Include(x => x.Followers.Where(y => y.FollowerId == userId))
                                            .Where(x => model.TargetUsersId.Contains(x.Id))
                                            .ToListAsync();

            foreach (var participant in chat.Participants)
            {
                if (participant.UserId != userId)
                _context.ChatParticipants.Remove(participant);
            }
            await _context.SaveChangesAsync();

            foreach (var user in users)
            {
                if (userId != user.Id && (!user.PrivateAccount && user.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
            || user.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString())
            )
                {
                    chat.Participants.Add(new ChatParticipant()
                    {
                        State = true,
                        UserId = user.Id
                    });
                }
            }
            await _context.SaveChangesAsync();


            //var targetUser = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == userId))
            //                                     .FirstOrDefaultAsync(x => x.Id == targetUserId && x.IsActive);
            //if (targetUser == default)
            //    throw new UserNotFoundException();
            //if (!targetUser.PrivateAccount && targetUser.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
            //|| targetUser.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
            //|| userId == targetUserId)
            //{
            //    chat.Participants.Add(new ChatParticipant()
            //    {
            //        State = null,
            //        UserId = targetUserId
            //    });
            //    await _context.SaveChangesAsync();
            //}
            //else
            //    throw new UserDontHaveAccessException();
        }

        public async Task<List<MessageModel>> GetChat(Guid userId, ChatRequestModel model)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var chat = await _context.Chats.Include(x => x.Participants)
                                           .FirstOrDefaultAsync(x => x.IsActive && x.Id == model.ChatId && x.Participants.Any(y => y.UserId == userId));
            if (chat == null)
                throw new ChatNotFoundException();
            var result = new List<MessageModel>();
            await _context.Messages.Include(x => x.Author)
                                        .ThenInclude(x => x.Avatar)
                                    .Where(x => x.IsActive && x.ChatId == model.ChatId)
                                    .OrderByDescending(x => x.Created)
                                    .Skip(model.Skip)
                                    .Take(model.Take)
                                    .ForEachAsync(x =>
                                    {
                                        if (x.AuthorId != userId && !x.State)
                                            x.State = true; //помечает сообщения как прочитанные
                                        result.Add(_mapper.Map<MessageModel>(x));
                                    });
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<List<ChatModel>> GetChats(Guid userId, int skip, int take)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var result = await _context.Chats.Include(x => x.Messages.OrderByDescending(x => x.Created).Where(x => x.IsActive).Take(1))
                                    .ThenInclude(x => x.Author)
                                        .ThenInclude(x => x.Avatar)
                                    .Include(x => x.Participants)
                                        .ThenInclude(x => x.User)
                                            .ThenInclude(x => x.Avatar)
                                            //.Include(x => x.Participants)
                                .Where(x => /* (!x.IsPrivate || x.Messages.Where(y => y.IsActive).Count() > 0 ) &&*/ x.IsActive && x.Participants.Any(y => y.UserId == userId && y.State == true))
                                .OrderBy(x => x.Messages.Where(y => y.IsActive).Count() == 0)                                          //пустые чаты в конце
                                .ThenByDescending(x => x.Messages.OrderByDescending(y => y.Created).First().Created) //затем сортировка по времени последнего сообщения
                                .Skip(skip)
                                .Take(take)
                                .Select(x => _mapper.Map<ChatModel>(x))
                                .ToListAsync();
            return result;
        }


        public async Task<ChatModel?> GetChatData(Guid userId, Guid chatId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var chat = await _context.Chats.Include(x => x.Messages.OrderByDescending(x => x.Created).Where(x => x.IsActive).Take(1))
                                    .ThenInclude(x => x.Author)
                                        .ThenInclude(x => x.Avatar)
                                    .Include(x => x.Participants)
                                        .ThenInclude(x => x.User)
                                            .ThenInclude(x => x.Avatar)
                                .FirstOrDefaultAsync(x => x.Id == chatId && x.IsActive && x.Participants.Any(y => y.UserId == userId && y.State == true));
                                
            if (chat != null)
            {
                var chatModel = _mapper.Map<ChatModel>(chat);
                return chatModel;
            }
           return null;
        }

        public async Task<List<UserWithAvatarLinkModel>> GetChatParticipants(Guid chatId)
        {
            var result = await _context.ChatParticipants.AsNoTracking()
                                                        .Include(x => x.User)
                                                        .ThenInclude(x => x.Avatar)
                                                        .Where(x => x.ChatId == chatId && x.State == true)
                                                        .Select(x => x.User)
                                                        .Select(x => _mapper.Map<UserWithAvatarLinkModel>(x))
                                                        .ToListAsync()
                                                        ; //TODO: проверка существует ли чат
            return result;
        }

        public async Task<List<ChatModel>> GetChatRequests(Guid userId, int skip, int take)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var result = await _context.Chats.Where(x => x.IsActive && x.Participants.Any(y => y.UserId == userId && y.State == null))
                                             .Skip(skip)
                                             .Take(take)
                                             .Select(x => _mapper.Map<ChatModel>(x))
                                             .ToListAsync();
            return result;
        }

        public async Task AcceptRequest(Guid userId, Guid chatId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var chat = await _context.ChatParticipants.Include(x => x.Chat)
                .FirstOrDefaultAsync(x => x.ChatId == chatId && x.UserId == userId && x.State == null);
            if (chat == null)
                throw new ChatNotFoundException();
            chat.State = true;
            await _context.SaveChangesAsync();
        }

        public async Task SendMessage(Guid userId, CreateMessageModel messageModel)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var chat = await _context.Chats.Include(x => x.Participants)
                                           .FirstOrDefaultAsync(x => x.IsActive && x.Id == messageModel.ChatId && x.Participants.Any(y => y.UserId == userId && y.State == true));
            if (chat == null)
                throw new ChatNotFoundException();
            var message = _mapper.Map<Message>(messageModel);
            message.AuthorId = userId;
            chat.Messages.Add(message);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteMessage(Guid userId, Guid messageId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var message = await _context.Messages
                .FirstOrDefaultAsync(x => x.IsActive && x.Id == messageId && x.AuthorId == userId);
            if (message == null)
                throw new MessageNotFoundException();
            message.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
