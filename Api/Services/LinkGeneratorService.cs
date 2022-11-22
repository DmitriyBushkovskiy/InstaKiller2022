using Api.Models.Attach;
using Api.Models.PostContent;
using Api.Models.User;
using DAL.Entities;
using System.IO;

namespace Api.Services
{
    public class LinkGeneratorService
    {
        public Func<User, string?>? LinkAvatarGenerator { get; set; }
        public Func<PostContent, string?>? LinkContentGenerator { get; set; }
        public void GetAvatarLink(User s, UserWithAvatarLinkModel d)
        {
            d.AvatarLink = s.Avatar == null ? null : LinkAvatarGenerator?.Invoke(s);
        }
        public void GetContentLink(PostContent s, PostContentModel d)
        {
            d.ContentLink = LinkContentGenerator?.Invoke(s);
        }
        public void GetAttachLink(MetadataModel s, Attach d)
        {
            d.FilePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "attaches"), s.TempId.ToString());
        }
    }
}
