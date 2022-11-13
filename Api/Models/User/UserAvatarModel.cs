namespace Api.Models.User
{
    public class UserAvatarModel : UserModel
    {
        public string? AvatarLink { get; set; }
        public UserAvatarModel(UserModel model, Func<UserModel, string?>? linkGenerator)
        {
            Id = model.Id;
            Username = model.Username;
            Email = model.Email;
            BirthDate = model.BirthDate;
            AvatarLink = linkGenerator?.Invoke(model);
        }
    }
}
