using Picterest.DTO.User;

namespace Picterest.Services.Interface
{
    public interface IJwtService
    {
        Task<string> GenerateAccessToken(UserDetails user);
    }
}
