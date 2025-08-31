using AutoMapper;
using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.AiResponseHistory.Request;
using IntelliPM.Data.DTOs.AiResponseHistory.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.AiHistory;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.AiResponseHistoryRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiResponseHistoryServices
{
    public class AiResponseHistoryService : IAiResponseHistoryService
    {
        private readonly IMapper _mapper;
        private readonly IAiResponseHistoryRepository _aiResponseHistoryRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly ILogger<AiResponseHistoryService> _logger;
        private readonly IDecodeTokenHandler _decodeToken;
        private readonly Su25Sep490IntelliPmContext _context;

        public AiResponseHistoryService(
            IMapper mapper,
            IAiResponseHistoryRepository aiResponseHistoryRepo,
            IProjectRepository projectRepo,
            IAccountRepository accountRepo,
            ILogger<AiResponseHistoryService> logger,
            IDecodeTokenHandler decodeToken,
            Su25Sep490IntelliPmContext context)
        {
            _mapper = mapper;
            _aiResponseHistoryRepo = aiResponseHistoryRepo;
            _projectRepo = projectRepo;
            _accountRepo = accountRepo;
            _logger = logger;
            _decodeToken = decodeToken;
            _context = context;
        }

        public async Task<List<AiResponseHistoryResponseDTO>> GetAllAsync()
        {
            var entities = await _aiResponseHistoryRepo.GetAllAsync();
            return _mapper.Map<List<AiResponseHistoryResponseDTO>>(entities);
        }

        public async Task<AiResponseHistoryResponseDTO> GetByIdAsync(int id)
        {
            var entity = await _aiResponseHistoryRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"AI response history with ID {id} not found.");

            return _mapper.Map<AiResponseHistoryResponseDTO>(entity);
        }

        public async Task<List<AiResponseHistoryResponseDTO>> GetByAiFeatureAsync(string aiFeature)
        {
            if (string.IsNullOrEmpty(aiFeature))
                throw new ArgumentException("AI feature cannot be null or empty.");

            var validFeatures = await _context.DynamicCategory
                .Where(dc => dc.CategoryGroup == "ai_feature")
                .Select(dc => dc.Name)
                .ToListAsync();
            if (!validFeatures.Contains(aiFeature))
                throw new ArgumentException($"Invalid AI feature: {aiFeature}. Valid features are: {string.Join(", ", validFeatures)}");

            var entities = await _aiResponseHistoryRepo.GetByAiFeatureAsync(aiFeature);
            if (!entities.Any())
                throw new KeyNotFoundException($"No AI response history found for AI feature '{aiFeature}'.");

            return _mapper.Map<List<AiResponseHistoryResponseDTO>>(entities);
        }

        public async Task<List<AiResponseHistoryResponseDTO>> GetByProjectIdAsync(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Project ID must be greater than 0.");

            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");

            var entities = await _aiResponseHistoryRepo.GetByProjectIdAsync(projectId);
            if (!entities.Any())
                throw new KeyNotFoundException($"No AI response history found for Project ID {projectId}.");

            return _mapper.Map<List<AiResponseHistoryResponseDTO>>(entities);
        }

        public async Task<List<AiResponseHistoryResponseDTO>> GetByCreatedByAsync(int createdBy)
        {
            var account = await _accountRepo.GetAccountById(createdBy);
            if (account == null)
                throw new KeyNotFoundException($"Account with ID {createdBy} not found.");

            var entities = await _aiResponseHistoryRepo.GetByCreatedByAsync(createdBy);
            if (!entities.Any())
                throw new KeyNotFoundException($"No AI response history found for CreatedBy ID {createdBy}.");

            return _mapper.Map<List<AiResponseHistoryResponseDTO>>(entities);
        }

        public async Task<AiResponseHistoryResponseDTO> CreateAsync(AiResponseHistoryRequestDTO request, string token)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.AiFeature))
                throw new ArgumentException("AI feature is required.", nameof(request.AiFeature));

            if (string.IsNullOrEmpty(request.ResponseJson))
                throw new ArgumentException("Response JSON is required.", nameof(request.ResponseJson));

            var decode = _decodeToken.Decode(token);
            if (decode == null || string.IsNullOrEmpty(decode.username))
                throw new UnauthorizedAccessException("Invalid token data.");

            var currentAccount = await _accountRepo.GetAccountByUsername(decode.username);
            if (currentAccount == null)
                throw new KeyNotFoundException("User not found.");

            var validFeatures = await _context.DynamicCategory
                .Where(dc => dc.CategoryGroup == "ai_feature")
                .Select(dc => dc.Name)
                .ToListAsync();
            if (!validFeatures.Contains(request.AiFeature))
                throw new ArgumentException($"Invalid AI feature: {request.AiFeature}. Valid features are: {string.Join(", ", validFeatures)}");

            if (request.ProjectId.HasValue)
            {
                var project = await _projectRepo.GetByIdAsync(request.ProjectId.Value);
                if (project == null)
                    throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");
            }

            var entity = _mapper.Map<AiResponseHistory>(request);
            entity.CreatedBy = currentAccount.Id;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Status = AiHistoryStatusEnum.ACTIVE.ToString();

            try
            {
                await _aiResponseHistoryRepo.AddAsync(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create AI response history: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create AI response history: {ex.Message}", ex);
            }

            return _mapper.Map<AiResponseHistoryResponseDTO>(entity);
        }

        public async Task<AiResponseHistoryResponseDTO> UpdateAsync(int id, AiResponseHistoryRequestDTO request)
        {
            var entity = await _aiResponseHistoryRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"AI response history with ID {id} not found.");

            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (!string.IsNullOrEmpty(request.AiFeature))
            {
                var validFeatures = await _context.DynamicCategory
                    .Where(dc => dc.CategoryGroup == "ai_feature")
                    .Select(dc => dc.Name)
                    .ToListAsync();
                if (!validFeatures.Contains(request.AiFeature))
                    throw new ArgumentException($"Invalid AI feature: {request.AiFeature}. Valid features are: {string.Join(", ", validFeatures)}");
            }

            if (request.ProjectId.HasValue)
            {
                var project = await _projectRepo.GetByIdAsync(request.ProjectId.Value);
                if (project == null)
                    throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");
            }

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _aiResponseHistoryRepo.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update AI response history: {ex.Message}", ex);
            }

            return _mapper.Map<AiResponseHistoryResponseDTO>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _aiResponseHistoryRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"AI response history with ID {id} not found.");

            try
            {
                await _aiResponseHistoryRepo.DeleteAsync(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete AI response history: {ex.Message}", ex);
            }
        }
    }
}
