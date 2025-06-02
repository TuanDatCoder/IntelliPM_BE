using AutoMapper;
using Data.DTOs.Account;
using Data.DTOs.Admin;
using Data.DTOs.Brand;
using Data.Enums;
using Repositories.AccountRepos;
using Repositories.BrandRepos;
using Repositories.OrderRepos;
using Repositories.StoreRepos;
using Repositories.TransactionRepos;
using Services.BrandServices;
using Services.EmailServices;
using Services.Helper.CustomExceptions;
using Services.Helper.DecodeTokenHandler;
using Services.Helper.VerifyCode;
using Services.JWTServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Services.AdminServices
{
    public class AdminService: IAdminService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;
        private readonly IDecodeTokenHandler _decodeToken;

        public AdminService(IAccountRepository accountRepository, IOrderRepository orderRepository, IStoreRepository storeRepository, ITransactionRepository transactionRepository, IMapper mapper, IDecodeTokenHandler decodeToken)
        {
            _accountRepository = accountRepository;
            _orderRepository = orderRepository;
            _storeRepository = storeRepository;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
            _decodeToken = decodeToken;
        }
        public async Task<AdminDashboardDTO> GetDashboardStatsAsync()
        {
            // 1. Tính Total Revenue (tổng tiền order * 10%)
            var orders = await _orderRepository.GetAllOrdersAsync(); 
            var totalOrderAmount = orders?.Sum(order => order.TotalPrice) ?? 0m; // Tổng tiền tất cả order
            var totalRevenue = totalOrderAmount * 0.1m; // 10% của tổng tiền

            // 2. Tính Total Users (tổng số người dùng)
            var totalUsers = await _accountRepository.GetTotalUsersAsync(); // Giả sử có phương thức này

            // 3. Tính Total Stores (tổng số cửa hàng)
            var totalStores = await _storeRepository.GetTotalStoresAsync(); 

            return new AdminDashboardDTO
            {
                TotalRevenue = totalRevenue,
                TotalUsers = totalUsers,
                TotalStores = totalStores
            };
        }

        public async Task<List<AccountResponseDTO>> GetAllAccountsAsync(int? page, int? size)
        {
            // Phân quyền đã được xử lý ở [Authorize] trong controller
            var accounts = await _accountRepository.GetAccounts(page, size);
            return _mapper.Map<List<AccountResponseDTO>>(accounts);
        }

    }
}
