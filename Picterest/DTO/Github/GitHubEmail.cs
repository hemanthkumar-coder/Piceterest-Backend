using System.Text.Json.Serialization;

namespace Picterest.DTO.Github
{
    public class GitHubEmail
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("primary")]
        public bool Primary { get; set; }

        [JsonPropertyName("verified")]
        public bool Verified { get; set; }

        [JsonPropertyName("visibility")]
        public string? Visibility { get; set; }
    }
}
