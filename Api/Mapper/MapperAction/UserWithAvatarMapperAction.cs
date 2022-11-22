using Api.Models.User;
using Api.Services;
using AutoMapper;
using DAL.Entities;

namespace Api.Mapper.MapperAction
{
    public class UserWithAvatarMapperAction : IMappingAction<User, UserWithAvatarLinkModel>
    {
        private LinkGeneratorService _linkGeneratorService;

        public UserWithAvatarMapperAction(LinkGeneratorService linkGeneratorService)
        {
            _linkGeneratorService = linkGeneratorService;
        }

        public void Process(User source, UserWithAvatarLinkModel destination, ResolutionContext context) 
            => _linkGeneratorService.GetAvatarLink(source, destination);
    }
}
