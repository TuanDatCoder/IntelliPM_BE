using IntelliPM.Data.DTOs.Account.Request;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.Admin.Request;
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
        Task Logout(string refreshToken);
        Task RegisterCustomer(LoginRequestDTO loginRequestDTO);
       Task ResetPassword(ResetPasswordRequestDTO resetPasswordRequestDTO);
        string GenerateOTP();
        Task<LoginResponseDTO> LoginGoogle(LoginGoogleRequestDTO loginGoogleRequestDTO);
        Task VerifyAccount(string token);
        Task AccountRegister(AccountRequestDTO accountRequestDTO);

        Task<string> AdminAccountRegister(AdminAccountRequestDTO accountRequestDTO);
    }
}
