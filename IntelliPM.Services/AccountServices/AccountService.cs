using AutoMapper;
using IntelliPM.Data.DTOs.Account;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Services.AuthenticationServices;
using IntelliPM.Services.CloudinaryStorageServices; // Thay Firebase bằng Cloudinary
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.Helper.CustomExceptions;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliPM.Services.AccountServices
{
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly IAccountRepository _accountRepository;
        private readonly IDecodeTokenHandler _decodeTokenHandler;
        private readonly IEmailService _emailService;
        private readonly ICloudinaryStorageService _cloudinaryStorageService; // Thay Firebase bằng Cloudinary
        private readonly IAuthenticationService _authenticationService;

        public AccountService(
            IMapper mapper,
            IAccountRepository accountRepository,
            IDecodeTokenHandler decodeTokenHandler,
            IEmailService emailService,
            ICloudinaryStorageService cloudinaryStorageService, // Thay Firebase bằng Cloudinary
            IAuthenticationService authenticationService)
        {
            _mapper = mapper;
            _accountRepository = accountRepository;
            _decodeTokenHandler = decodeTokenHandler;
            _emailService = emailService;
            _cloudinaryStorageService = cloudinaryStorageService;
            _authenticationService = authenticationService;
        }

        public async Task<string> UploadProfilePictureAsync(int accountId, Stream fileStream, string fileName)
        {
            var account = await _accountRepository.GetAccountById(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            var fileUrl = await _cloudinaryStorageService.UploadFileAsync(fileStream, fileName);
            account.Picture = fileUrl;
            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepository.Update(account);

            return fileUrl;
        }

        public async Task<string> UploadPictureAsync(string token, Stream fileStream, string fileName)
        {
            var account = await _authenticationService.GetAccountByToken(token);
            var fileUrl = await _cloudinaryStorageService.UploadFileAsync(fileStream, fileName);
            account.Picture = fileUrl;
            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepository.Update(account);

            return fileUrl;
        }

        public async Task<bool> CheckUsernameExisted(string username)
        {
            return await _accountRepository.IsExistedByUsername(username);
        }

        public async Task<bool> CheckEmailExisted(string email)
        {
            return await _accountRepository.IsExistedByEmail(email);
        }

        public async Task<AccountResponseDTO> ChangeAccountStatus(int id, string newStatus)
        {
            var existingAccount = await _accountRepository.GetAccountById(id);
            if (existingAccount == null)
            {
                throw new KeyNotFoundException($"Account with ID {id} not found.");
            }

            existingAccount.Status = newStatus.ToString();
            await _accountRepository.Update(existingAccount);
            return _mapper.Map<AccountResponseDTO>(existingAccount);
        }

    }
}