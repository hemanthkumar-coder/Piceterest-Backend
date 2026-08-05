namespace Picterest.Models.User
{
    public class CreateUserModel
    {
        public long GithubId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
