using Api.Models.Token;
using Api.Models.User;
using Api.Services;
using Common;
using Common.Extentions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;

        public AuthController(AuthService authService, UserService userService, PostService postService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<TokenModel> GetToken(TokenRequestModel model)
            => await _authService.GetToken(model.Login, model.Pass);

        [HttpPost]
        public async Task<TokenModel> RefreshToken(RefreshTokenRequestModel model)
            => await _authService.GetTokenByRefreshToken(model.RefreshToken);

        [HttpPost]
        public async Task RegisterUser(CreateUserModel model)
        {
            if (await _userService.CheckUserExist(model.Email, model.Username))
                throw new Exception("user is exist");
            await _userService.CreateUser(model);
        }
    }
}
