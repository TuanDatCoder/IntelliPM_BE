using AutoMapper;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.Admin;
using IntelliPM.Data.DTOs.Admin.Request;
using IntelliPM.Data.DTOs.Admin.Response;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Services.AuthenticationServices;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.Helper.CustomExceptions;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using IntelliPM.Services.Helper.DynamicCategoryHelper;

namespace IntelliPM.Services.AdminServices
{
    public class AdminService : IAdminService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;
        private readonly IDecodeTokenHandler _decodeToken;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEmailService _emailService;
        private readonly IDynamicCategoryHelper _dynamicCategoryHelper;

        public AdminService(IAccountRepository accountRepository, IMapper mapper, IDecodeTokenHandler decodeToken, IProjectRepository projectRepository, IAuthenticationService authenticationService, IEmailService emailService, IDynamicCategoryHelper dynamicCategoryHelper)

        {
            _accountRepository = accountRepository;
            _projectRepository = projectRepository;
            _mapper = mapper;
            _decodeToken = decodeToken;
            _authenticationService = authenticationService;
            _emailService = emailService;
            _dynamicCategoryHelper = dynamicCategoryHelper;
        }

        public async Task<List<AccountResponseDTO>> GetAllAccountsAsync()
        {

            var accounts = await _accountRepository.GetAccounts();
            return _mapper.Map<List<AccountResponseDTO>>(accounts);
        }

        public async Task<List<ProjectStatusReportDto>> GetProjectStatusReportsAsync()
        {
            var calculationMode = await _dynamicCategoryHelper.GetCategoryNameAsync("calculation_mode", "SYSTEM");
            return await _projectRepository.GetAllProjectStatusReportsAsync(calculationMode);
        }

        public async Task<List<ProjectManagerReportDto>> GetProjectManagerReportsAsync()
        {
            var reports = await _projectRepository.GetProjectManagerReportsAsync();
            return reports;
        }


        public async Task<AdminRegisterResponseDTO> RegisterAccountAsync(List<AdminAccountRequestDTO> requests)
        {
            var response = new AdminRegisterResponseDTO();

            foreach (var request in requests)
            {
                try
                {
                    // Call AuthenticationService to register a single account
                    var email = await _authenticationService.AdminAccountRegister(request);
                    response.Successful.Add(email);
                }
                catch (ApiException ex)
                {
                    response.Failed.Add(new RegistrationError
                    {
                        Email = request.Email,
                        ErrorMessage = ex.Message
                    });
                }
                catch (Exception ex)
                {
                    response.Failed.Add(new RegistrationError
                    {
                        Email = request.Email,
                        ErrorMessage = "An unexpected error occurred"
                    });
                }
            }

            return response;
        }




    }
}
