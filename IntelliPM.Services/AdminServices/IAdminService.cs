using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.Admin;
using IntelliPM.Data.DTOs.Admin.Request;
using IntelliPM.Data.DTOs.Admin.Response;
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
        Task<List<ProjectStatusReportDto>> GetProjectStatusReportsAsync();
        Task<List<ProjectManagerReportDto>> GetProjectManagerReportsAsync();
        Task<AdminRegisterResponseDTO> RegisterAccountAsync(List<AdminAccountRequestDTO> requests);
    }
}
