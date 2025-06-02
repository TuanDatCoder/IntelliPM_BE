using AutoMapper;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Repositories.AccountRepos;
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
        private readonly IMapper _mapper;
        private readonly IDecodeTokenHandler _decodeToken;

        public AdminService(IAccountRepository accountRepository,  IMapper mapper, IDecodeTokenHandler decodeToken)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
            _decodeToken = decodeToken;
        }
       
        public async Task<List<AccountResponseDTO>> GetAllAccountsAsync(int? page, int? size)
        {
            // Phân quyền đã được xử lý ở [Authorize] trong controller
            var accounts = await _accountRepository.GetAccounts(page, size);
            return _mapper.Map<List<AccountResponseDTO>>(accounts);
        }

    }
}
