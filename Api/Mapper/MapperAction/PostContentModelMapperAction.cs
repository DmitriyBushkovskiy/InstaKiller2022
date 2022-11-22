using Api.Models.PostContent;
using Api.Models.User;
using Api.Services;
using AutoMapper;
using DAL.Entities;

namespace Api.Mapper.MapperAction
{
    public class PostContentModelMapperAction : IMappingAction<PostContent, PostContentModel>
    {
        private LinkGeneratorService _linkGeneratorService;

        public PostContentModelMapperAction(LinkGeneratorService linkGeneratorService)
        {
            _linkGeneratorService = linkGeneratorService;
        }

        public void Process(PostContent source, PostContentModel destination, ResolutionContext context) 
           => _linkGeneratorService.GetContentLink(source, destination);
    }
}
