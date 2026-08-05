using Picterest.DbModels;

namespace Picterest.Repositories.Interface
{
    public interface IUserRepository : IGenericRepository<User>
    {
        User? GetUserByGithubId(long githubId);
    }
}
