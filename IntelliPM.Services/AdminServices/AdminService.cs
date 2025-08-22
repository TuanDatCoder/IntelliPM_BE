using AutoMapper;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.Admin;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AdminServices
{
    public class AdminService: IAdminService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;
        private readonly IDecodeTokenHandler _decodeToken;

        public AdminService(IAccountRepository accountRepository, IMapper mapper, IDecodeTokenHandler decodeToken, IProjectRepository projectRepository)
        {
            _accountRepository = accountRepository;
            _projectRepository = projectRepository;
            _mapper = mapper;
            _decodeToken = decodeToken;
        }
       
        public async Task<List<AccountResponseDTO>> GetAllAccountsAsync()
        {
           
            var accounts = await _accountRepository.GetAccounts();
            return _mapper.Map<List<AccountResponseDTO>>(accounts);
        }

        public async Task<List<ProjectStatusReportDto>> GetProjectStatusReportsAsync()
        {
            return await _projectRepository.GetAllProjectStatusReportsAsync();
        }

        public async Task<List<ProjectManagerReportDto>> GetProjectManagerReportsAsync()
        {
            var reports = await _projectRepository.GetProjectManagerReportsAsync();
            return reports;
        }
    }
}
