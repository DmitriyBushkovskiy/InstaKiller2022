using Api.Models.PostContent;
using Api.Models.User;
using Api.Services;
using AutoMapper;
using Common.Consts;
using DAL.Entities;

namespace Api.Mapper.MapperAction
{
    public class PostContentModelMapperAction : IMappingAction<PostContent, PostContentModel>
    {
        private LinkGeneratorService _linkGeneratorService;
        private PostService _postService;

        public PostContentModelMapperAction(LinkGeneratorService linkGeneratorService, PostService postService)
        {
            _linkGeneratorService = linkGeneratorService;
            _postService = postService;
        }

        public void Process(PostContent source, PostContentModel destination, ResolutionContext context)
        {
            _linkGeneratorService.GetContentLink(source, destination);
            destination.LikedByMe = source.Likes.Any(x => x.UserId == _postService.UserId?.Invoke(ClaimNames.Id));
        }
    }
}
