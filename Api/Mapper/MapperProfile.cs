using Api.Controllers;
using Api.Mapper.MapperAction;
using Api.Models.Attach;
using Api.Models.Chat;
using Api.Models.Comment;
using Api.Models.Message;
using Api.Models.Post;
using Api.Models.PostContent;
using Api.Models.Relation;
using Api.Models.User;
using AutoMapper;
using Common;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;

namespace Api.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateUserModel, User>()
                .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
                .ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime))
                .ForMember(d => d.Registered, m => m.MapFrom(s => DateTimeOffset.UtcNow))
                ;

            CreateMap<User, UserModel>()
                .ForMember(d => d.PostsAmount, m => m.MapFrom(s => s.Posts.Count()))
                .ForMember(d => d.FollowedAmount, m => m.MapFrom(s => s.Followed.Count()))
                .ForMember(d => d.FollowersAmount, m => m.MapFrom(s => s.Followers.Count()))
                ;

            CreateMap<Avatar, AttachModel>();
            CreateMap<PostContent, AttachModel>();

            CreateMap<User, UserWithAvatarLinkModel>()
                .ForMember(d => d.PostsAmount, m => m.MapFrom(s => s.Posts.Where(x => x.IsActive).Count()))
                .ForMember(d => d.FollowedAmount, m => m.MapFrom(s => s.Followed.Count()))
                .ForMember(d => d.FollowersAmount, m => m.MapFrom(s => s.Followers.Count()))
                .AfterMap<UserWithAvatarMapperAction>();

            CreateMap<PostContent, PostContentModel>()
                .ForMember(d => d.Likes, m => m.MapFrom(s => s.Likes.Count))
                .AfterMap<PostContentModelMapperAction>();

                
            CreateMap<Comment, CommentModel>()
                .ForMember(d => d.Likes, m => m.MapFrom(s => s.Likes.Count))
                .AfterMap<CommentModelMapperAction>();


            CreateMap<CreateComment, Comment>()
                .ForMember(d => d.Created, m => m.MapFrom(s => DateTimeOffset.UtcNow));

            CreateMap<Post, PostModel>()
                .ForMember(d => d.Likes, m => m.MapFrom(s => s.Likes.Count))
                .ForMember(d => d.PostContent, m => m.MapFrom(s => s.Content))
                .AfterMap<PostModelMapperAction>();

            CreateMap<ChangeUserDataModel, User>();

            CreateMap<User, UserDataModel>() 
                ;

            CreateMap<MetadataModel, Avatar>()
                .AfterMap<AttachMapperAction>();

            CreateMap<CreatePostModel, Post>()
                .ForMember(d => d.Created, m => m.MapFrom(s => DateTimeOffset.UtcNow))
                .AfterMap<PostMapperAction>();
 
            CreateMap<MetadataModel, PostContent>()
                .AfterMap<AttachMapperAction>();

            CreateMap<Relation, FollowedModel>();

            CreateMap<Relation, FollowerModel>();

            CreateMap<CreatePostRequest, CreatePostModel>();

            CreateMap<CreateMessageModel, Message>()
                .ForMember(d => d.Created, m => m.MapFrom(s => DateTimeOffset.UtcNow));

            CreateMap<Message, MessageModel>();

            CreateMap<Chat, ChatModel>()
                .ForMember(d => d.LastMessage, m => m.MapFrom(s => s.Messages.OrderByDescending(x => x.Created).FirstOrDefault()));

            CreateMap<User, RelationStateModel>()
                .ForMember(d => d.TargetUser, m => m.MapFrom(s => s))
                .AfterMap<RelationsModelMapperAction>();
            ;
            ;
        }
    }
}
