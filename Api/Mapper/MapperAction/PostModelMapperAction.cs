using Api.Models.Post;
using Api.Services;
using AutoMapper;
using Common.Consts;
using DAL.Entities;

namespace Api.Mapper.MapperAction
{
    public class PostModelMapperAction : IMappingAction<Post, PostModel>
    {
        private PostService _postService;

        public PostModelMapperAction(PostService postService)
        {
            _postService = postService;
        }

        public void Process(Post source, PostModel destination, ResolutionContext context)
        {
            destination.LikedByMe = source.Likes.Any(x => x.UserId == _postService.UserId?.Invoke(ClaimNames.Id));
        }
    }
}
