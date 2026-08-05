using Picterest.DTO.Github;
using Picterest.HelperModels;

namespace Picterest.Services.Interface
{
    public interface IAuthService
    {
        Task<ServiceResult> GetGithubAccessToken(string code);
        Task<ServiceResult> GetUserInfo(string accessToken);
        Task<ServiceResult> GetGithubUserEmail(string accessToken);
        Task<ServiceResult> BuildUserDetails(string accessToken);
        Task<ServiceResult> CreateUserIfNotExistsOrGetUserDetails(GithubUserDetails userDetails);
    }
}
