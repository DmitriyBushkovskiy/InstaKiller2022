using Api.Controllers;
using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Post;
using Api.Models.PostContent;
using Api.Models.User;
using AutoMapper;
using Common;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;

namespace Api
{
    public class MapperProfile: Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateUserModel, User>()
                .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
                .ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime))
                .ForMember(d => d.Registered, m => m.MapFrom(s => DateTimeOffset.UtcNow))
                ;


            CreateMap<User, UserModel>();
            CreateMap<Avatar, AttachModel>();
            CreateMap<PostContent, AttachModel>();



            CreateMap<User, UserWithAvatarLinkModel>()
                .ForMember(d => d.AvatarLink, m => m.MapFrom(s => LinkGenerateHelper.LinkAvatarGenerator ==null || s.Avatar == null ? null : LinkGenerateHelper.LinkAvatarGenerator(s)));
            CreateMap<PostContent, PostContentModel>()
                .ForMember(d => d.ContentLink, m => m.MapFrom(s => LinkGenerateHelper.LinkContentGenerator == null ? null : LinkGenerateHelper.LinkContentGenerator(s)));
            CreateMap<Comment, CommentModel>();
            CreateMap<CreateComment, Comment>()
                .ForMember(d => d.Created, m => m.MapFrom(s => DateTimeOffset.UtcNow));
            CreateMap<Post, PostModel>();
            CreateMap<ChangeUserDataModel, User>();
            CreateMap<User, UserDataModel>();
            CreateMap<MetadataModel, PostContent>();
            CreateMap<MetadataModel, Avatar>();
        }
    }
}
