using Picterest.HelperModels;
using Picterest.Models.User;

namespace Picterest.Services.Interface
{
    public interface IUserService
    {
        Task<ServiceResult> GetUserWithGithubId(long githubId);
        Task<ServiceResult> CreateUser(CreateUserModel createUserModel);
    }
}
