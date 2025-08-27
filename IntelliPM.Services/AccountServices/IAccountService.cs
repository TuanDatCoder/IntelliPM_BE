using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.Project.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  IntelliPM.Services.AccountServices
{
    public interface IAccountService
    {
        Task<string> UploadProfilePictureAsync(int accountId, Stream fileStream, string fileName);
        Task<string> UploadPictureAsync(string token, Stream fileStream, string fileName);
        Task<AccountResponseDTO> ChangeAccountStatus(int id, string newStatus);
        Task<AccountResponseDTO> GetAccountByEmail(string email);
        Task<AccountWithWorkItemDTO> GetAccountAndWorkItemById(int accountId);
        Task<ProfileResponseDTO> GetProfileByEmail(string email);
        Task<AccountResponseDTO> GetProfileById(int id);
    }
}
