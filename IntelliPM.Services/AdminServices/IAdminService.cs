using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AdminServices
{
    public interface IAdminService
    {
        Task<List<AccountResponseDTO>> GetAllAccountsAsync();
    }
}
