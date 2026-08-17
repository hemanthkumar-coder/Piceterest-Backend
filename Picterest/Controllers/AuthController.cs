using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Picterest.DbModels;
using Picterest.DTO.Github;
using Picterest.DTO.User;
using Picterest.Models.User;
using Picterest.Services.Implementation;
using Picterest.Services.Interface;
using System.Security.Claims;

namespace Picterest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController: ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private ILogger<AuthController> _logger;
        public AuthController(IConfiguration configuration, IAuthService authService,IJwtService jwtService, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _authService = authService;
            _logger = logger;
            _jwtService = jwtService;
        }

        [HttpGet("github-login")]
        public async Task<IActionResult> Login()
        {
            var clientId = _configuration["githubClientId"] ?? throw new InvalidOperationException("GitHub client ID is not configured");
            var redirectUrl = _configuration["RedirectUrl"] ?? throw new InvalidOperationException("Redirect URL is not configured");
            var scope = _configuration["scope"] ?? throw new InvalidOperationException("Scope is not configured");

            // Implementation for GitHub login redirection
            return Redirect($"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={redirectUrl}&scope={scope}&state={Guid.NewGuid()}");
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code, string state)
        {
            if (code == null || state == null)
            {
                return BadRequest("Code or state is null");
            }

            var authServiceResponse = await _authService.GetGithubAccessToken(code);

            if (authServiceResponse == null)
            {
                new JsonResult(new
                {
                    Success = false,
                    Message = "Failed to get access token from GitHub",
                    StatusCode = 500
                });
            }

            if (authServiceResponse != null && !authServiceResponse.IsSuccess)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = authServiceResponse.Error,
                    StatusCode = authServiceResponse.StatusCode
                });
            }

            var AccessTokenResponse = authServiceResponse?.Result != null ? authServiceResponse.Result as GithubAccessTokenResponse :null;
            if (AccessTokenResponse == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Failed to parse access token response",
                    StatusCode = 500
                });
            }

            var accessToken = AccessTokenResponse.AccessToken;
            if (accessToken == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Access token is null",
                    StatusCode = 500
                });
            }

            var userDetailsServiceResponse = await _authService.BuildUserDetails(accessToken);
            if (userDetailsServiceResponse == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Failed to build user details",
                    StatusCode = 500
                });
            }

            if(!userDetailsServiceResponse.IsSuccess)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = userDetailsServiceResponse.Error,
                    StatusCode = userDetailsServiceResponse.StatusCode
                });
            }

            var userDetails = userDetailsServiceResponse.Result != null ? userDetailsServiceResponse.Result as GithubUserDetails : null;
            if(userDetails == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Failed to parse user details",
                    StatusCode = 500
                });
            }

            var createUserOrGetDetailsResponse = await _authService.CreateUserIfNotExistsOrGetUserDetails(userDetails);
            if (createUserOrGetDetailsResponse == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Something Went Wrong",
                    StatusCode = 500
                });
            }


            if (!createUserOrGetDetailsResponse.IsSuccess)
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = createUserOrGetDetailsResponse.Error,
                    StatusCode = createUserOrGetDetailsResponse.StatusCode
                });
            }

            var dbUserDetails = createUserOrGetDetailsResponse.Result != null ? createUserOrGetDetailsResponse.Result as UserDetails : null;
            if(dbUserDetails == null)
            {
                _logger.LogError("Error Parsing UserDetails From Service Result");
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Something Went Wrong",
                    StatusCode = 500
                });
            }


            var token = await _jwtService.GenerateAccessToken(dbUserDetails);
            if (string.IsNullOrEmpty(token))
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "Error Authenticating the User",
                    StatusCode = 500
                });
            }

            var ExpirationTime = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes");

            Response.Cookies.Append(
                    "access_token",
                    token,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false, // false only for local HTTP testing
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(ExpirationTime),
                        Path="/"
                    });

            var homePageUrl = _configuration["HomePageUrl"] ?? throw new InvalidOperationException("HomePage Url is not Configured");
            return Redirect(homePageUrl);



        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userDetails = new
            {
                Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Name = User.FindFirst(ClaimTypes.Name)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                AvatarUrl = User.FindFirst("avatar_url")?.Value
            };
            
            return new JsonResult(new
            {
                Success = true,
                Message = "Fetched User Details",
                StatusCode = 200,
                Data = userDetails
            });
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> logout()
        {
            Response.Cookies.Delete("access_token", new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            return new JsonResult(new
            {
                Success = true,
                Message = "Logout Successful",
                StatusCode = 200
            });
        }
    }
}
