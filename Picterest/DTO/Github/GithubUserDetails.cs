namespace Picterest.DTO.Github
{
    public class GithubUserDetails
    {
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public long GithubId { get; set; }

    }
}
