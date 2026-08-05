using System.Text.Json.Serialization;

namespace Picterest.DTO.Github
{
    public class GithubAccessTokenRequest
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = string.Empty;
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = string.Empty;
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
    }
}
