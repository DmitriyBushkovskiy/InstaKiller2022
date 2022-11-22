using Api.Models.Post;
using Api.Models.PostContent;
using Api.Services;
using AutoMapper;
using DAL.Entities;

namespace Api.Mapper.MapperAction
{
    public class PostMapperAction : IMappingAction<CreatePostModel, Post>
    {
        public void Process(CreatePostModel source, Post destination, ResolutionContext context)
        {
            destination.Content.ToList().ForEach(x => x.AuthorId = (Guid)source.AuthorId!);
        }
    }
}
