using Api.Models.Comment;
using Api.Models.Post;
using Api.Services;
using AutoMapper;
using Common.Consts;
using DAL.Entities;

namespace Api.Mapper.MapperAction
{
    public class CommentModelMapperAction : IMappingAction<Comment, CommentModel>
    {
        private PostService _postService;

        public CommentModelMapperAction(PostService postService)
        {
            _postService = postService;
        }

        public void Process(Comment source, CommentModel destination, ResolutionContext context)
        {
            destination.LikedByMe = source.Likes.Any(x => x.UserId == _postService.UserId?.Invoke(ClaimNames.Id));
        }
    }
}