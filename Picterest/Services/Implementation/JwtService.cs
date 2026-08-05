using Microsoft.IdentityModel.Tokens;
using Picterest.DTO.User;
using Picterest.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Picterest.Services.Implementation
{
    public class JwtService : IJwtService
    {
        private readonly ILogger<JwtService> _logger;
        private readonly IConfiguration _configuration;
        public JwtService( ILogger<JwtService> logger,IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<string> GenerateAccessToken(UserDetails user)
        {
            try
            {
                var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new("GithubId", user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email ?? ""),
                new Claim("avatar_url", user.AvatarUrl.ToString() ?? string.Empty)
            };

                var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt Key is not Specified");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var Issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt Issuer is not Specified");
                var Audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt Audience is not Specified");
                var ExpirationTime = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes");

                var token = new JwtSecurityToken(Issuer, Audience, claims, expires: DateTime.UtcNow.AddMinutes(ExpirationTime), signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error While Generating Access Token");
                return string.Empty;
            }

        }
    }
}
