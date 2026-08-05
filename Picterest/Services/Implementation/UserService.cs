using Picterest.DbModels;
using Picterest.DTO.User;
using Picterest.HelperModels;
using Picterest.Models.User;
using Picterest.Repositories.Interface;
using Picterest.Services.Interface;

namespace Picterest.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;
        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServiceResult> CreateUser(CreateUserModel createUserModel)
        {
            if(createUserModel == null)
            {
                _logger.LogError("Create User Model is null");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 400
                };
            }

            try
            {
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Name = createUserModel.Name,
                    Email = createUserModel.Email,
                    AvatarUrl = createUserModel.AvatarUrl,
                    GithubId = createUserModel.GithubId,
                    GithubUserName = createUserModel.Name,
                    CreatedAt = DateTime.UtcNow,
                };

                await _unitOfWork.BeginTransactionAsync();

                await _unitOfWork.Users.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                var userDetails = new UserDetails
                {
                    Name = createUserModel.Name,
                    Id = newUser.Id,
                    Email = createUserModel.Email,
                    AvatarUrl = createUserModel.AvatarUrl
                };

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "User Created Successfully",
                    Result = userDetails
                };

            }
            catch (Exception ex)
            {
                await _unitOfWork.RevertTransactionAsync();
                _logger.LogError(ex, "Something Went Wrong While Creating User");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }



        }

        public async Task<ServiceResult> GetUserWithGithubId(long githubId)
        {
            try
            {
                var user = _unitOfWork.Users.GetUserByGithubId(githubId);

                if (user == null)
                {
                    _logger.LogWarning($"User with GithubId {githubId} not found.");
                    return new ServiceResult
                    {
                        IsSuccess = true,
                        Message = "User not found.",
                        StatusCode = 404
                    };
                }

                var userDetails = new UserDetails
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl
                };

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "User found.",
                    StatusCode = 200,
                    Result = userDetails
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something Went Wrong Retrieving User Details with GithubId");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "An error occurred while retrieving user details.",
                    StatusCode = 500
                };
            }
        }
    }
}
