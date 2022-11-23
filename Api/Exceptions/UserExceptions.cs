namespace Api.Exceptions
{
    public class NotFoundException : Exception
    {
        public string? Model { get; set; }
        public override string Message => $"{Model} is not found";
    }

    public class UserNotFoundException : NotFoundException
    {
        public UserNotFoundException()
        {
            Model = "User";
        }
    }

    public class PostNotFoundException : NotFoundException
    {
        public PostNotFoundException()
        {
            Model = "Post";
        }
    }

    public class ChatNotFoundException : NotFoundException
    {
        public ChatNotFoundException()
        {
            Model = "Chat";
        }
    }

    public class MessageNotFoundException : NotFoundException
    {
        public MessageNotFoundException()
        {
            Model = "Message";
        }
    }

    public class CommentNotFoundException : NotFoundException
    {
        public CommentNotFoundException()
        {
            Model = "Comment";
        }
    }

    public class ContentNotFoundException : NotFoundException
    {
        public ContentNotFoundException()
        {
            Model = "Content";
        }
    }

    public class UserNotAuthorizedException : Exception
    {
        public override string Message => $"You are not authorized";
    }

    public class UserDontHaveAccessException : Exception
    {
        public override string Message => $"You don't have access";
    }
}