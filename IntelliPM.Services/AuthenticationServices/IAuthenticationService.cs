using IntelliPM.Data.DTOs.Account.Request;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.Auth.Request;
using IntelliPM.Data.DTOs.Auth.Response;
using IntelliPM.Data.DTOs.Password;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AuthenticationServices
{
    public interface IAuthenticationService
    {
        System.Threading.Tasks.Task ChangePassword(string token, ChangePasswordRequestDTO changePasswordRequestDTO);
        System.Threading.Tasks.Task ForgotPassword(string email);
        Task<AccountInformationResponseDTO> GetUserInfor(string token);
        Task<Account> GetAccountByToken(string token);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        System.Threading.Tasks.Task Logout(string refreshToken);
        System.Threading.Tasks.Task RegisterCustomer(LoginRequestDTO loginRequestDTO);
        System.Threading.Tasks.Task ResetPassword(ResetPasswordRequestDTO resetPasswordRequestDTO);
        string GenerateOTP();
        Task<LoginResponseDTO> LoginGoogle(LoginGoogleRequestDTO loginGoogleRequestDTO);
        System.Threading.Tasks.Task VerifyAccount(string token);
        System.Threading.Tasks.Task AccountRegister(AccountRequestDTO accountRequestDTO);
    }
}
