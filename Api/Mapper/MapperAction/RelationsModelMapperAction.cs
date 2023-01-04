using Api.Models.Post;
using Api.Models.Relation;
using Api.Services;
using AutoMapper;
using Common.Consts;
using Common.Enums;
using DAL.Entities;

namespace Api.Mapper.MapperAction
{
    public class RelationsModelMapperAction : IMappingAction<User, RelationStateModel>
    {
        private RelationService _postService;

        public RelationsModelMapperAction(RelationService postService)
        {
            _postService = postService;
        }

        public void Process(User source, RelationStateModel destination, ResolutionContext context)
        {
            destination.RelationAsFollower = source.Followers.FirstOrDefault(x => x.FollowerId == _postService.UserId?.Invoke(ClaimNames.Id))?.State ?? RelationState.NotState.ToString();
            destination.RelationAsFollowed = source.Followed.FirstOrDefault(x => x.FollowedId == _postService.UserId?.Invoke(ClaimNames.Id))?.State ?? RelationState.NotState.ToString();
        }
    }
}
