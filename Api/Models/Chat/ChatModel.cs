﻿using Api.Models.Message;
using Api.Models.User;
using DAL.Entities;

namespace Api.Models.Chat
{
    public class ChatModel
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public Guid CreatorId { get; set; }
        public bool IsPrivate { get; set; }
        public string? Name { get; set; }
        public MessageModel? LastMessage { get; set; }
        public virtual List<UserWithAvatarLinkModel> Participants { get; set; } = null!;
    }
}
