using IntelliPM.Data.DTOs.Account.Response;
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
    }
}
