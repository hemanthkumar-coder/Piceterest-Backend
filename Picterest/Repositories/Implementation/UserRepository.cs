using Picterest.Context;
using Picterest.DbModels;
using Picterest.Repositories.Interface;

namespace Picterest.Repositories.Implementation
{
    public class UserRepository : GenericRepository<User, ImageDbContext>, IUserRepository
    {
        public UserRepository(ImageDbContext context) : base(context)
        {
        }
        public User? GetUserByGithubId(long githubId)
        {
            return _context.Users.FirstOrDefault(u => u.GithubId == githubId);
        }
    }
}
