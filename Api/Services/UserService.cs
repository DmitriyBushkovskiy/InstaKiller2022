using Api.Models.Attach;
using Api.Models.User;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
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
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                var avatar = new Avatar 
                {   
                    Author = user, 
                    MimeType = model.MimeType, 
                    FilePath = filePath, 
                    Name = model.Name, 
                    Size = model.Size , 
                    User = user, 
                    UserID = user.Id 
                };
                user.Avatar = avatar;
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
            var users = await _context.Users.Include(x => x.Avatar).AsNoTracking().ToListAsync();  //TODO: если переписать в одну строчку?  применить селект сразу к этой строке?
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
            if (user == null)
                throw new Exception("user not found");
            return user;
        }
    }
}
