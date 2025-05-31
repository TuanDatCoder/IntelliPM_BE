using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.Account;
using IntelliPM.Data.DTOs.Auth;
using IntelliPM.Data.DTOs.Password;
using IntelliPM.Data.DTOs.RefreshToken.Request;
using IntelliPM.Services.JWTServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IntelliPM.Services.AccountServices;
using IntelliPM.Services.AuthenticationServices;
using System.Net;
using IntelliPM.Services.Helper.CustomExceptions;

namespace ConstructionEquipmentRental.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IJWTService _jwtService;
        private readonly IAccountService _accountService;
        private readonly IAuthenticationService _authenticationService;
        private readonly string _frontendUrl;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config,IJWTService jwtService, IAccountService accountService, IAuthenticationService authenticationService)
        {
            _jwtService = jwtService;
            _accountService = accountService;
            _authenticationService = authenticationService;
            _config = config;
             #pragma warning disable CS8601
            _frontendUrl = config["Environment:FE_URL"];
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterAccount(AccountRequestDTO accountRequestDTO)
        {

            // await _accountService.AccountRegister(accountRequestDTO);
            await _authenticationService.AccountRegister(accountRequestDTO);
            ApiResponseDTO response = new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Register new account successfully",
            };

            return StatusCode(response.Code, response);
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var result = await _authenticationService.Login(loginRequestDTO);

            ApiResponseDTO response = new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Login successfully",
                Data = result,
            };

            return StatusCode(response.Code, response);
        }

        [HttpPost]
        [Route("login-google")]
        public async Task<IActionResult> LoginGoogle([FromBody] LoginGoogleRequestDTO loginGoogleRequestDTO)
        {
            var result = await _authenticationService.LoginGoogle(loginGoogleRequestDTO);

            ApiResponseDTO response = new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Login google successfully",
                Data = result,
            };

            return StatusCode(response.Code, response);
        }

        [HttpGet]
        [Authorize]
        [Route("user-infor")]
        public async Task<IActionResult> GetUserInfor()
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];

            var result = await _authenticationService.GetUserInfor(token);

            ApiResponseDTO response = new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Get user information successfully",
                Data = result,
            };

            return StatusCode(response.Code, response);
        }

        [HttpPost]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO forgotPasswordRequestDTO)
        {
            await _authenticationService.ForgotPassword(forgotPasswordRequestDTO.email);

            ApiResponseDTO response = new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = $"Send OTP code to {forgotPasswordRequestDTO.email} successfully",
            };

            return StatusCode(response.Code, response);
        }

        [HttpPost]
        [Authorize]
        [Route("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDTO changePasswordReqModel)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];

            await _authenticationService.ChangePassword(token, changePasswordReqModel);

            ApiResponseDTO response = new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Change password successfully",
            };

            return StatusCode(response.Code, response);
        }

        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDTO resetPasswordRequestDTO)
        {
            await _authenticationService.ResetPassword(resetPasswordRequestDTO);

            ApiResponseDTO response = new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Reset password successfully",
            };

            return StatusCode(response.Code, response);
        }


        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout(RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            await _authenticationService.Logout(refreshTokenRequestDTO.RefreshToken);

            ApiResponseDTO response = new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Logout successfully",
            };

            return StatusCode(response.Code, response);
        }


        [HttpGet]
        [Route("verify")]
        public async Task<IActionResult> VerifyAccount([FromQuery] string token)
        {
            try
            {
                await _authenticationService.VerifyAccount(token);
                return Redirect($"{_frontendUrl}/verify-success");
            }
            catch (ApiException ex)
            {
                return Redirect($"{_frontendUrl}/verify-fail");
            }
        }



    }
}
