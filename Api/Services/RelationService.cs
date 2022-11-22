using Api.Configs;
using Api.Controllers;
using Api.Models.Relation;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;

namespace Api.Services
{
    public class RelationService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly AuthConfig _config;

        public RelationService(IMapper mapper, IOptions<AuthConfig> config, DataContext context)
        {
            _mapper = mapper;
            _context = context;
            _config = config.Value;
        }

        /// <summary>
        /// State = true - подписан
        /// State = null - подал заявку на подписку (у закрытых аккаунтов)
        /// State = false - забаненый аккаунт
        /// </summary>

        public async Task<List<FollowedModel>> GetFollowed(Guid userId, Guid targetUserId)
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
                var result = await _context.Relations.AsNoTracking()
                                     .Where(x => x.FollowerId == targetUserId && x.Followed.IsActive == true
                                            && (!x.Followed.PrivateAccount && x.State != false || x.State == true))
                                     .Include(x => x.Followed.Avatar)
                                     .Include(x => x.Followed)
                                     .Select(x => _mapper.Map<FollowedModel>(x))
                                     .ToListAsync();
                return result;
            }
            throw new Exception("you don't have access");
        }

        public async Task<List<FollowerModel>> GetFollowersRequests(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new Exception("user not found");
            if (!user.PrivateAccount)
                return new List<FollowerModel>();
            var result = await _context.Relations.Include(x => x.Follower)
                                                .Include(x => x.Follower.Avatar)
                                                .Where(x => x.FollowedId == userId && x.State == null && x.Follower.IsActive)
                                                .Select(x => _mapper.Map<FollowerModel>(x))
                                                .ToListAsync();
            return result;
        }

        public async Task<List<FollowerModel>> GetBanned(Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new Exception("user not found");
            var result = await _context.Relations.Include(x => x.Follower)
                                                .Include(x => x.Follower.Avatar)
                                                .Where(x => x.FollowedId == userId && x.State == false && x.Follower.IsActive)
                                                .Select(x => _mapper.Map<FollowerModel>(x))
                                                .ToListAsync();
            return result;
        } 

        public async Task<List<FollowerModel>> GetFollowers(Guid userId, Guid targetUserId)
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
                var result = await _context.Relations.Include(x => x.Follower)
                                                    .Include(x => x.Follower.Avatar)
                                                    .Where(x => x.FollowedId == targetUserId && x.State == true && x.Follower.IsActive)
                                                    .Select(x => _mapper.Map<FollowerModel>(x))
                                                    .ToListAsync();
                return result;
            }
            throw new Exception("you don't have access");
        }

        public async Task<bool?> Follow(Guid userId, Guid targetUserId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive == true))
                throw new Exception("user not found");

            var targetUser = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == userId))
                                                 .FirstOrDefaultAsync(x => x.Id == targetUserId && x.IsActive);
            if (targetUser == default)
                throw new Exception("target user not found");
            var relation = targetUser.Followers.FirstOrDefault();
            if (relation == null)
            {
                relation = new Relation() { FollowedId = targetUserId, FollowerId = userId };
                if (targetUser.PrivateAccount)
                    relation.State = null;
                else
                    relation.State = true;
                await _context.Relations.AddAsync(relation);
                await _context.SaveChangesAsync();
                return relation.State;
            }
            else
            {
                if (relation.State == false)
                    throw new Exception("you are banned");
                _context.Relations.Remove(relation);
                await _context.SaveChangesAsync();
                return false;
            }
        }

        public async Task Ban(Guid userId, Guid targetUserId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new Exception("user not found");
            var user = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == targetUserId))
                                     .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new Exception("target user not found");
            var relation = user.Followers.FirstOrDefault();

            if (relation == null)
            {
                relation = new Relation() { FollowedId = userId, FollowerId = targetUserId, State = false };
                await _context.Relations.AddAsync(relation);
            }
            else
                relation.State = false;
            await _context.SaveChangesAsync();
        }

        public async Task Unban(Guid userId, Guid targetUserId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new Exception("user not found");
            var user = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == targetUserId))
                                     .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new Exception("target user not found");
            var relation = user.Followers.FirstOrDefault();
            if (relation == null || relation.State != false)
                throw new Exception("user not banned");
            else
            {
                relation.State = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
