using Api.Models.Attach;
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

        public async Task<bool> CheckUserExist(string email)
        {
            //TODO: проверка по емайлу и юзернейму
            return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

        public async Task AddAvatarToUser(Guid userId, MetadataModel model, string filePath)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId); // TODO: написать покороче с использованием автермапа
            if (user != null)
            {
                user.Avatar = _mapper.Map<MetadataModel, Avatar>(model, opts => opts.AfterMap((s, d) => 
                {
                    d.Author = user;
                    d.FilePath = filePath;
                    d.User = user;
                    d.UserID = user.Id;
                }));
                await _context.SaveChangesAsync();
            }
        }

        public async Task<AttachModel> GetUserAvatar(Guid userId)
        {
            var user = await GetUserById(userId);
            return _mapper.Map<AttachModel>(user.Avatar);
        }

        public async Task Delete(Guid id)
        {
            var dbUser = await GetUserById(id);
            if (dbUser != null)
            {
                //TODO: неполное удаление юзера
                _context.Users.Remove(dbUser);
                await _context.SaveChangesAsync();
            }
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
            var users = await _context.Users.AsNoTracking().Include(x => x.Avatar).ToListAsync();
            return users.Select(x => _mapper.Map<UserWithAvatarLinkModel>(x));
        }
        public async Task<UserWithAvatarLinkModel> GetUser(Guid id)
        {
            var user = await GetUserById(id);
            return _mapper.Map<UserWithAvatarLinkModel>(user);
        }

        private async Task<User> GetUserById(Guid id)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == id);
            if (user == default)
                throw new Exception("user not found");
            return user;
        }

        public async Task ChangeUserData(ChangeUserDataModel data, Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new Exception("user not found");
            _mapper.Map<ChangeUserDataModel, User>(data, user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserDataModel> GetUserData(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new Exception("user not found");
            return _mapper.Map<UserDataModel>(user);
        }

        public async Task ChangeUsername(ChangeUsernameModel data, Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new Exception("user not found");
            if (await _context.Users.AnyAsync(x => x.Username == data.Username))
                throw new Exception("username is already taken");
            user.Username = data.Username;
            await _context.SaveChangesAsync();
        }

        public async Task ChangeEmail(ChangeEmailModel data, Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new Exception("user not found");
            if (await _context.Users.AnyAsync(x => x.Email == data.Email))
                throw new Exception("username is already taken");
            user.Username = data.Email;
            await _context.SaveChangesAsync();
        }
    }
}
