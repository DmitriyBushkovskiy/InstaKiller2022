using Api.Configs;
using Api.Controllers;
using Api.Exceptions;
using Api.Models.Relation;
using Api.Models.User;
using AutoMapper;
using Common.Enums;
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
        public Func<string, Guid>? UserId { get; set; }

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
                throw new UserNotFoundException();
            var targetUser = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == userId))
                                     .FirstOrDefaultAsync(x => x.Id == targetUserId && x.IsActive);
            if (targetUser == default)
                throw new UserNotFoundException();

            if (!targetUser.PrivateAccount && targetUser.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
            || targetUser.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
            || userId == targetUserId)
            {
                var result = await _context.Relations.AsNoTracking()
                                     .Where(x => x.FollowerId == targetUserId && x.Followed.IsActive == true
                                            && (!x.Followed.PrivateAccount && x.State != RelationState.Banned.ToString() || x.State == RelationState.Follower.ToString()))
                                     .Include(x => x.Followed.Avatar)
                                     .Include(x => x.Followed)
                                     .Select(x => _mapper.Map<FollowedModel>(x))
                                     .ToListAsync();
                return result;
            }
            throw new UserDontHaveAccessException();
        }

        public async Task<List<FollowerModel>> GetFollowersRequests(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new UserNotFoundException();
            if (!user.PrivateAccount)
                return new List<FollowerModel>();
            var result = await _context.Relations.Include(x => x.Follower)
                                                .Include(x => x.Follower.Avatar)
                                                .Where(x => x.FollowedId == userId && x.State == null && x.Follower.IsActive)
                                                .Select(x => _mapper.Map<FollowerModel>(x))
                                                .ToListAsync();
            return result;
        }

        public async Task<string> GetMyRelationState(Guid userId, Guid targetUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new UserNotFoundException();
            var result = await _context.Relations.Include(x => x.Follower)
                                                .FirstOrDefaultAsync(x => x.FollowedId == targetUserId && x.FollowerId == userId && x.Follower.IsActive);
            return result?.State ?? RelationState.NotState.ToString();
        }

        public async Task<string> GetRelationToMeState(Guid userId, Guid targetUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new UserNotFoundException();
            var result = await _context.Relations.Include(x => x.Follower)
                                                .FirstOrDefaultAsync(x => x.FollowedId == userId && x.FollowerId == targetUserId && x.Follower.IsActive);
            return result?.State ?? RelationState.NotState.ToString();
        }

        public async Task<RelationStateModel?> GetRelations(Guid userId, Guid targetUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new UserNotFoundException();

            var targetUser = await _context.Users.Include(x => x.Avatar)
                                                .Include(x => x.Posts.Where(x => x.IsActive))
                                                .Include(x => x.Followers)
                                                .Include(x => x.Followed)
                                                .FirstOrDefaultAsync(x => x.Id == targetUserId && x.IsActive);

            if (targetUser != null)
            {
                var relationsModel = new RelationStateModel() {
                    TargetUser = _mapper.Map<UserWithAvatarLinkModel>(targetUser), 
                    RelationAsFollower = targetUser.Followers.FirstOrDefault(x => x.FollowerId == userId)?.State ?? RelationState.NotState.ToString(),
                    RelationAsFollowed = targetUser.Followed.FirstOrDefault(x => x.FollowedId == userId)?.State ?? RelationState.NotState.ToString(),
                };
                return relationsModel;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<RelationStateModel>> SearchUsers(Guid userId, SearchUsersRequestModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new UserNotFoundException();

            var result = await _context.Users.AsNoTracking()
                .Include(x => x.Avatar)
                .Include(x => x.Posts.Where(x => x.IsActive))
                .Include(x => x.Followers)
                .Include(x => x.Followed)
                .Where(x => x.Username.Contains(model.Username) && x.Id != userId)
                .Skip(model.Skip)
                .Take(model.Take)
                .Select(x => _mapper.Map<RelationStateModel>(x))
                .ToListAsync();
            return result;
        }

        public async Task<List<FollowerModel>> GetBanned(Guid userId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var result = await _context.Relations.Include(x => x.Follower)
                                                .Include(x => x.Follower.Avatar)
                                                .Where(x => x.FollowedId == userId && x.State == RelationState.Banned.ToString() && x.Follower.IsActive)
                                                .Select(x => _mapper.Map<FollowerModel>(x))
                                                .ToListAsync();
            return result;
        }

        public async Task<List<FollowerModel>> GetFollowers(Guid userId, Guid targetUserId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var targetUser = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == userId))
                                     .FirstOrDefaultAsync(x => x.Id == targetUserId && x.IsActive);
            if (targetUser == default)
                throw new UserNotFoundException();
            if (!targetUser.PrivateAccount && targetUser.Followers.FirstOrDefault()?.State != RelationState.Banned.ToString()
            || targetUser.Followers.FirstOrDefault()?.State == RelationState.Follower.ToString()
            || userId == targetUserId)
            {
                var result = await _context.Relations.Include(x => x.Follower)
                                                    .Include(x => x.Follower.Avatar)
                                                    .Where(x => x.FollowedId == targetUserId && x.State == RelationState.Follower.ToString() && x.Follower.IsActive)
                                                    .Select(x => _mapper.Map<FollowerModel>(x))
                                                    .ToListAsync();
                return result;
            }
            throw new UserDontHaveAccessException();
        }

        public async Task<string> Follow(Guid userId, Guid targetUserId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive == true))
                throw new UserNotFoundException();

            var targetUser = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == userId))
                                                 .FirstOrDefaultAsync(x => x.Id == targetUserId && x.IsActive);
            if (targetUser == default)
                throw new UserNotFoundException();
            var relation = targetUser.Followers.FirstOrDefault();
            if (relation == null)
            {
                relation = new Relation() { FollowedId = targetUserId, FollowerId = userId };
                if (targetUser.PrivateAccount)
                    relation.State = RelationState.Request.ToString();
                else
                    relation.State = RelationState.Follower.ToString();
                await _context.Relations.AddAsync(relation);
                await _context.SaveChangesAsync();
                return relation.State;
            }
            else
            {
                if (relation.State == RelationState.Banned.ToString())
                    throw new Exception("you are banned");
                _context.Relations.Remove(relation);
                await _context.SaveChangesAsync();
                return RelationState.NotState.ToString();
            }
        }

        public async Task<string> Ban(Guid userId, Guid targetUserId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var user = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == targetUserId))
                                     .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new UserNotFoundException();
            var relation = user.Followers.FirstOrDefault();

            if (relation == null)
            {
                relation = new Relation() { FollowedId = userId, FollowerId = targetUserId, State = RelationState.Banned.ToString() };
                await _context.Relations.AddAsync(relation);

            }
            else
                relation.State = RelationState.Banned.ToString();
            await _context.SaveChangesAsync();
            return relation.State;
        }

        public async Task<string> Unban(Guid userId, Guid targetUserId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var user = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == targetUserId))
                                     .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new UserNotFoundException();
            var relation = user.Followers.FirstOrDefault();
            if (relation == null || relation.State != RelationState.Banned.ToString())
                throw new Exception("user not banned");
            else
            {
                relation.State = RelationState.NotState.ToString();
                await _context.SaveChangesAsync();
            }
            return relation.State;
        }

        public async Task<string> AcceptRequest(Guid userId, Guid targetUserId)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == userId && x.IsActive))
                throw new UserNotFoundException();
            var user = await _context.Users.Include(x => x.Followers.Where(y => y.FollowerId == targetUserId))
                                     .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive);
            if (user == default)
                throw new UserNotFoundException();
            var relation = user.Followers.FirstOrDefault();

            if (relation?.State == RelationState.Request.ToString())
            {
                relation.State = RelationState.Follower.ToString();
                await _context.SaveChangesAsync();
                return relation.State;
            }
            else
                throw new Exception("You didn't send request");
        }
    }
}