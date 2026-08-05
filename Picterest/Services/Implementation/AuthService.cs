using Microsoft.AspNetCore.Mvc;
using Picterest.DTO.Github;
using Picterest.DTO.User;
using Picterest.HelperModels;
using Picterest.Models.User;
using Picterest.Services.Interface;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Picterest.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;
        public AuthService(IConfiguration configuration, ILogger<AuthService> logger, HttpClient httpClient,IUserService userService)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _userService = userService;
        }

        public async Task<ServiceResult> BuildUserDetails(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Access token is null or empty");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Access token is null or empty",
                    StatusCode = 400
                };
            }
            try
            {
                var GetEmailTask =  GetGithubUserEmail(accessToken);
                var GetUserInfoTask =  GetUserInfo(accessToken);

                await Task.WhenAll(GetEmailTask, GetUserInfoTask);

                var emailResult = await GetEmailTask;
                var userInfoResult = await GetUserInfoTask;

                if(emailResult == null)
                {
                    _logger.LogError("Email Result is null");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong",
                        StatusCode = 500
                    };
                }
                if(userInfoResult == null)
                {
                    _logger.LogError("UserInfo Result is null");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong",
                        StatusCode = 500
                    };
                }
                if(!emailResult.IsSuccess)
                {
                    _logger.LogError($"Error Getting User Email: {emailResult.Error}");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = emailResult.Error,
                        StatusCode = emailResult.StatusCode
                    };
                }
                if (!userInfoResult.IsSuccess)
                {
                    _logger.LogError($"Error Getting User Info: {userInfoResult.Error}");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = userInfoResult.Error,
                        StatusCode = userInfoResult.StatusCode
                    };
                }

                var userEmail = emailResult != null ? emailResult.Result as string : "";
                var userInfo = userInfoResult != null ? userInfoResult.Result as UserInfoResponse : null;
                

                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogError("User Email is null or empty");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong",
                        StatusCode = 500
                    };
                }
                if(userInfo == null)
                {
                    _logger.LogError("User Info is null");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong",
                        StatusCode = 500
                    };
                }

                


                var userDetails = new GithubUserDetails
                {
                    Login = userInfo?.Login ?? "",
                    Email = userEmail ?? "",
                    AvatarUrl = userInfo?.avatarUrl ?? "",
                    GithubId = userInfo?.Id ?? 0
                };
                _logger.LogInformation("Github User Details {@UserDetails}", userDetails);
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "User Details Built Successfully",
                    Result = userDetails
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something Went Wrong while Building UserDetails");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }


        }

        public async Task<ServiceResult> CreateUserIfNotExistsOrGetUserDetails(GithubUserDetails userDetails)
        {
            try
            {
                var userDetailsInDbResponse = await _userService.GetUserWithGithubId(userDetails.GithubId);
                if (userDetailsInDbResponse == null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong",
                        StatusCode = 500
                    };
                }

                if (!userDetailsInDbResponse.IsSuccess)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = userDetailsInDbResponse.Error,
                        StatusCode = userDetailsInDbResponse.StatusCode
                    };
                }

                var userDetailsInDb = userDetailsInDbResponse.Result != null ? userDetailsInDbResponse.Result as UserDetails : null;
                if (userDetailsInDb == null)
                {
                    //As User Details is null New user will be Created.
                    var createUserModel = new CreateUserModel
                    {
                        GithubId = userDetails.GithubId,
                        Name = userDetails.Login,
                        Email = userDetails.Email,
                        AvatarUrl = userDetails.AvatarUrl,
                    };

                    var createUserServiceResponse = await _userService.CreateUser(createUserModel);
                    if (createUserServiceResponse == null)
                    {
                        _logger.LogError("createUserServiceResponse is null");
                        return new ServiceResult
                        {
                            IsSuccess = false,
                            Error = "Something Went Wrong",
                            StatusCode = 500
                        };
                    }

                    if (!createUserServiceResponse.IsSuccess)
                    {
                        return new ServiceResult
                        {
                            IsSuccess = false,
                            Error = createUserServiceResponse.Error,
                            StatusCode = createUserServiceResponse.StatusCode
                        };
                    }

                    var newUserDetails = createUserServiceResponse.Result != null ? createUserServiceResponse.Result as UserDetails : null;
                    if (newUserDetails == null)
                    {
                        _logger.LogError("Something Went Wrong While Parsing New User Details");
                        return new ServiceResult
                        {
                            IsSuccess = false,
                            Error = "Something Went Wrong",
                            StatusCode = 500
                        };
                    }

                    return new ServiceResult
                    {
                        IsSuccess = true,
                        Message = "User Created Successfully",
                        Result = newUserDetails
                    };

                }

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "User Found",
                    Result = userDetailsInDb
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something Went Wrong");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Something Went Wrong",
                    StatusCode = 500
                };
            }
        }

        public async Task<ServiceResult> GetGithubAccessToken(string code)
        {
            if (code == null)
            {
                _logger.LogError("Code is null");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Code is null",
                    StatusCode = 400
                };
            }

            try
            {
                var clientId = _configuration["githubClientId"] ?? throw new InvalidOperationException("GitHub client ID is not configured");
                var clientSecret = _configuration["ClientSecret"] ?? throw new InvalidOperationException("GitHub client secret is not configured");
                var accessTokenUrl = _configuration["GitHub:AccessTokenUrl"] ?? throw new InvalidOperationException("GitHub access token URL is not configured");

                var request = new HttpRequestMessage(HttpMethod.Post, accessTokenUrl);
                request.Headers.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json")
                    );
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "code", code }
                });



                

                var response = await _httpClient.SendAsync(request);

                if (response == null)
                {
                    _logger.LogError("Response from GitHub is null");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Response from GitHub is null",
                        StatusCode = 500
                    };
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to get access token from GitHub. Status code: {response.StatusCode}");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = $"Failed to get access token from GitHub. Status code: {response.StatusCode}",
                        StatusCode = (int)response.StatusCode
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var accessTokenResponse = JsonSerializer.Deserialize<GithubAccessTokenResponse>(responseContent);
                _logger.LogInformation("Access Token Response: {@AccessTokenResponse}", accessTokenResponse);
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Access token retrieved successfully",
                    Result = accessTokenResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Getting Access Token");

                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something went wrong while getting access token",
                    StatusCode = 500
                };
            }
        }

        public async Task<ServiceResult> GetGithubUserEmail(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Access token is null or empty");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Access token is null or empty",
                    StatusCode = 400
                };
            }

            try
            {
                var GetUserEmailUrl = _configuration["GitHub:GetUserEmailUrl"] ?? throw new InvalidOperationException("GitHub user email URL is not configured");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                _httpClient.DefaultRequestHeaders.UserAgent.Add(
                                new ProductInfoHeaderValue("Picterest", "1.0"));

                var response = await _httpClient.GetAsync(GetUserEmailUrl);
                if(response == null)
                {
                    _logger.LogError("Response from GitHub is null");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something went wrong while getting user email",
                        StatusCode = 500
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                if(string.IsNullOrEmpty(json))
                {
                    _logger.LogError("User Email Response is null or empty");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong",
                        StatusCode = 500
                    };
                }

                var userEmailResponse = JsonSerializer.Deserialize<List<GitHubEmail>>(json);
                if (userEmailResponse == null)
                {
                    _logger.LogError("Error Deserializing User Email Response");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong",
                        StatusCode = 500
                    };
                }

                var primaryEmail = userEmailResponse.FirstOrDefault(email => email.Primary)?.Email;
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "User Email Retrieved Successfully",
                    Result = primaryEmail
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something Went Wrong While Getting User Email");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }
        }

        public async Task<ServiceResult> GetUserInfo(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Access token is null or empty");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Access token is null or empty",
                    StatusCode = 400
                };
            }

            try
            {
                var UserInfoUrl = _configuration["GitHub:GetUserInfoUrl"] ?? throw new InvalidOperationException("GitHub user info URL is not configured");


                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                _httpClient.DefaultRequestHeaders.UserAgent.Add(
                                new ProductInfoHeaderValue("Picterest", "1.0"));

                var response = await _httpClient.GetAsync(UserInfoUrl);

                if (response == null)
                {
                    _logger.LogError("Response from GitHub is null");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Response from GitHub is null",
                        StatusCode = 500
                    };
                }


                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(json))
                {
                    _logger.LogError("UserInfo Response is null or empty");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong",
                        StatusCode = 500
                    };
                }

                var userInfo = JsonSerializer.Deserialize<UserInfoResponse>(json);
                if (userInfo == null)
                {
                    _logger.LogError("Error Deserializing UserInfo");
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Error = "Something Went Wrong",
                        StatusCode = 500
                    };
                }

                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "User Info Retrieved Successfully",
                    Result = userInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something Went Wrong While Retrieving UserInfo");
                return new ServiceResult
                {
                    IsSuccess = false,
                    Error = "Something Went Wrong",
                    StatusCode = 500
                };
            }


        }
    }
}
