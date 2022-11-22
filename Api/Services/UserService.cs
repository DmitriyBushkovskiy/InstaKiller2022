﻿using Api.Models.Attach;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Numerics;

namespace Api.Services
{
    public class UserService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public UserService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<bool> CheckUserExist(string email, string username)
        {
            return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower() 
                                                   || x.Username.ToLower() == username.ToLower());
        }

        public async Task AddAvatarToUser(Guid userId, MetadataModel model)
        {
            var user = _context.Users.Include(x => x.Avatar)
                                     .FirstOrDefault(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new Exception("user not found");
            var avatar = _mapper.Map<Avatar>(model);
            avatar.UserID = avatar.AuthorId = userId;
            user.Avatar = avatar;
            await _context.SaveChangesAsync();
        }

        public async Task<AttachModel> GetUserAvatar(Guid userId) 
        {
            var user = await GetUserById(userId);
            return _mapper.Map<AttachModel>(user.Avatar);
        }

        public async Task DeleteUser(Guid userId)
        {
            var user = await GetUserById(userId);
            if (user == default)
                throw new Exception("user not found");
            user.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task<Guid> CreateUser(CreateUserModel model)
        {
            var dbUser = _mapper.Map<User>(model);
            var userEntity = await _context.Users.AddAsync(dbUser);
            await _context.SaveChangesAsync();
            return userEntity.Entity.Id;
        }

        public async Task<IEnumerable<UserWithAvatarLinkModel>> GetUsers()
        {
            return await _context.Users.AsNoTracking()
                                        .Where(x => x.IsActive)
                                        .Include(x => x.Avatar)
                                        .OrderByDescending(x => x.Registered)
                                        .Select(x => _mapper.Map<UserWithAvatarLinkModel>(x))
                                        .ToListAsync();
        }

        public async Task<UserWithAvatarLinkModel> GetUser(Guid id)
        {
            var user = await GetUserById(id);
            return _mapper.Map<UserWithAvatarLinkModel>(user);
        }

        private async Task<User> GetUserById(Guid userId)
        {
            var user = await _context.Users.Include(x => x.Avatar)
                                           .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new Exception("user not found");
            return user;
        }

        public async Task ChangeUserData(ChangeUserDataModel data, Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new Exception("user not found");
            _mapper.Map<ChangeUserDataModel, User>(data, user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserDataModel> GetUserData(Guid userId, Guid targetUserId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new Exception("user not found");
            var targetUser = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == userId))
                                                 .FirstOrDefaultAsync(x => x.Id == targetUserId && x.IsActive);
            if (targetUser == default)
                throw new Exception("target user not found");
            if (!targetUser.PrivateAccount && targetUser.Followers.FirstOrDefault()?.State != false
            || targetUser.Followers.FirstOrDefault()?.State == true
            || userId == targetUserId)
            {
                return _mapper.Map<UserDataModel>(targetUser);
            }
            throw new Exception("you don't have access");
        }

        public async Task ChangeUsername(ChangeUsernameModel data, Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new Exception("user not found");
            if (await _context.Users.AnyAsync(x => x.Username.ToLower() == data.Username.ToLower()))
                throw new Exception("username is already exist");
            user.Username = data.Username;
            await _context.SaveChangesAsync();
        }

        public async Task ChangeEmail(ChangeEmailModel data, Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new Exception("user not found");
            if (await _context.Users.AnyAsync(x => x.Email.ToLower() == data.Email.ToLower()))
                throw new Exception("email is already exist");
            user.Email = data.Email;
            await _context.SaveChangesAsync();
        }
    }
}
