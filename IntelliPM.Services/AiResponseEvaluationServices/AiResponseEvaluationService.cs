using AutoMapper;
using IntelliPM.Data.DTOs.AiResponseEvaluation.Request;
using IntelliPM.Data.DTOs.AiResponseEvaluation.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.AiResponseEvaluationRepos;
using IntelliPM.Repositories.AiResponseHistoryRepos;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiResponseEvaluationServices
{
    public class AiResponseEvaluationService : IAiResponseEvaluationService
    {
        private readonly IMapper _mapper;
        private readonly IAiResponseEvaluationRepository _aiResponseEvaluationRepo;
        private readonly IAiResponseHistoryRepository _aiResponseHistoryRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly ILogger<AiResponseEvaluationService> _logger;
        private readonly IDecodeTokenHandler _decodeToken;

        public AiResponseEvaluationService(
            IMapper mapper,
            IAiResponseEvaluationRepository aiResponseEvaluationRepo,
            IAiResponseHistoryRepository aiResponseHistoryRepo,
            IAccountRepository accountRepo,
            ILogger<AiResponseEvaluationService> logger,
            IDecodeTokenHandler decodeToken)
        {
            _mapper = mapper;
            _aiResponseEvaluationRepo = aiResponseEvaluationRepo;
            _aiResponseHistoryRepo = aiResponseHistoryRepo;
            _accountRepo = accountRepo;
            _logger = logger;
            _decodeToken = decodeToken;
        }

        public async Task<List<AiResponseEvaluationResponseDTO>> GetAllAsync()
        {
            var entities = await _aiResponseEvaluationRepo.GetAllAsync();
            return _mapper.Map<List<AiResponseEvaluationResponseDTO>>(entities);
        }

        public async Task<AiResponseEvaluationResponseDTO> GetByIdAsync(int id)
        {
            var entity = await _aiResponseEvaluationRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"AI response evaluation with ID {id} not found.");

            return _mapper.Map<AiResponseEvaluationResponseDTO>(entity);
        }

        public async Task<List<AiResponseEvaluationResponseDTO>> GetByAiResponseIdAsync(int aiResponseId)
        {
            var aiResponse = await _aiResponseHistoryRepo.GetByIdAsync(aiResponseId);
            if (aiResponse == null)
                throw new KeyNotFoundException($"AI response with ID {aiResponseId} not found.");

            var entities = await _aiResponseEvaluationRepo.GetByAiResponseIdAsync(aiResponseId);
            if (!entities.Any())
                throw new KeyNotFoundException($"No AI response evaluations found for AI response ID {aiResponseId}.");

            return _mapper.Map<List<AiResponseEvaluationResponseDTO>>(entities);
        }

        public async Task<List<AiResponseEvaluationResponseDTO>> GetByAccountIdAsync(int accountId)
        {
            var account = await _accountRepo.GetAccountById(accountId);
            if (account == null)
                throw new KeyNotFoundException($"Account with ID {accountId} not found.");

            var entities = await _aiResponseEvaluationRepo.GetByAccountIdAsync(accountId);
            if (!entities.Any())
                throw new KeyNotFoundException($"No AI response evaluations found for Account ID {accountId}.");

            return _mapper.Map<List<AiResponseEvaluationResponseDTO>>(entities);
        }

        public async Task<AiResponseEvaluationResponseDTO> CreateAsync(AiResponseEvaluationRequestDTO request, string token)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (request.AiResponseId <= 0)
                throw new ArgumentException("AI response ID must be greater than 0.", nameof(request.AiResponseId));

            if (request.Rating < 1 || request.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.", nameof(request.Rating));

            var decode = _decodeToken.Decode(token);
            if (decode == null || string.IsNullOrEmpty(decode.username))
                throw new UnauthorizedAccessException("Invalid token data.");

            var currentAccount = await _accountRepo.GetAccountByUsername(decode.username);
            if (currentAccount == null)
                throw new KeyNotFoundException("User not found.");

            var aiResponse = await _aiResponseHistoryRepo.GetByIdAsync(request.AiResponseId);
            if (aiResponse == null)
                throw new KeyNotFoundException($"AI response with ID {request.AiResponseId} not found.");

            var entity = _mapper.Map<AiResponseEvaluation>(request);
            entity.AccountId = currentAccount.Id;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _aiResponseEvaluationRepo.AddAsync(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create AI response evaluation: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create AI response evaluation: {ex.Message}", ex);
            }

            return _mapper.Map<AiResponseEvaluationResponseDTO>(entity);
        }

        public async Task<AiResponseEvaluationResponseDTO> UpdateAsync(int id, AiResponseEvaluationRequestDTO request)
        {
            var entity = await _aiResponseEvaluationRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"AI response evaluation with ID {id} not found.");

            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (request.AiResponseId <= 0)
                throw new ArgumentException("AI response ID must be greater than 0.", nameof(request.AiResponseId));

            if (request.Rating < 1 || request.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.", nameof(request.Rating));

            var aiResponse = await _aiResponseHistoryRepo.GetByIdAsync(request.AiResponseId);
            if (aiResponse == null)
                throw new KeyNotFoundException($"AI response with ID {request.AiResponseId} not found.");

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _aiResponseEvaluationRepo.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update AI response evaluation: {ex.Message}", ex);
            }

            return _mapper.Map<AiResponseEvaluationResponseDTO>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _aiResponseEvaluationRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"AI response evaluation with ID {id} not found.");

            try
            {
                await _aiResponseEvaluationRepo.DeleteAsync(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete AI response evaluation: {ex.Message}", ex);
            }
        }
    }
}
