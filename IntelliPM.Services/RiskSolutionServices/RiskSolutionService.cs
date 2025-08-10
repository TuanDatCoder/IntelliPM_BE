using AutoMapper;
using IntelliPM.Data.DTOs.Risk.Response;
using IntelliPM.Data.DTOs.RiskSolution.Request;
using IntelliPM.Data.DTOs.RiskSolution.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.RiskRepos;
using IntelliPM.Repositories.RiskSolutionRepos;
using IntelliPM.Services.ActivityLogServices;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RiskSolutionServices
{
    public class RiskSolutionService : IRiskSolutionService
    {
        private readonly IRiskRepository _riskRepo;
        private readonly IRiskSolutionRepository _repo;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;

        public RiskSolutionService(IRiskRepository riskRepo, IRiskSolutionRepository repo, IMapper mapper, IActivityLogService activityLogService)
        {
            _riskRepo = riskRepo;
            _repo = repo;
            _mapper = mapper;
            _activityLogService = activityLogService;
        }

        public async Task<List<RiskSolutionResponseDTO>> GetByRiskIdAsync(int riskId)
        {
            var data = await _repo.GetByRiskIdAsync(riskId);
            return _mapper.Map<List<RiskSolutionResponseDTO>>(data);
        }

        public async Task<RiskSolutionResponseDTO> CreateAsync(RiskSolutionRequestDTO dto)
        {
            if (dto.RiskId <= 0)
                throw new ArgumentException("RiskId must be a positive integer.");
            if (string.IsNullOrWhiteSpace(dto.MitigationPlan) && string.IsNullOrWhiteSpace(dto.ContingencyPlan))
                throw new ArgumentException("At least one of MitigationPlan or ContingencyPlan must be provided.");
            if (dto.CreatedBy <= 0)
                throw new ArgumentException("CreatedBy must be a positive integer.");

            var risk = await _riskRepo.GetByIdAsync(dto.RiskId)
                ?? throw new Exception("Risk not found with provided RiskId.");

            var entity = _mapper.Map<RiskSolution>(dto);
            entity.CreatedAt = entity.UpdatedAt = DateTime.UtcNow;

            await _repo.AddAsync(entity);
            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = risk.ProjectId,
                RiskKey = risk.RiskKey,
                RelatedEntityType = "Risk",
                RelatedEntityId = risk.RiskKey,
                ActionType = "CREATE",
                Message = $"Created risk solution in risk '{risk.RiskKey}'",
                CreatedBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow
            });
            return _mapper.Map<RiskSolutionResponseDTO>(entity);
        }

        public async Task<RiskSolutionResponseDTO?> UpdateContigencyPlanAsync(int id, string contigencyPlan, int createdBy)
        {
            if (string.IsNullOrWhiteSpace(contigencyPlan))
                throw new ArgumentException("ContingencyPlan must be provided.");
            if (createdBy <= 0)
                throw new ArgumentException("CreatedBy must be a positive integer.");

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;


            var risk = await _riskRepo.GetByIdAsync(existing.RiskId)
                ?? throw new Exception("Risk not found with provided RiskId.");

            existing.ContingencyPlan = contigencyPlan;
            existing.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _repo.UpdateAsync(existing);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = risk.ProjectId,
                    RiskKey = risk.RiskKey,
                    RelatedEntityType = "Risk",
                    RelatedEntityId = risk.RiskKey,
                    ActionType = "UPDATE",
                    Message = $"Updated risk contingency plan in risk '{risk.RiskKey}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change contigency plan: {ex.Message}", ex);
            }

            return _mapper.Map<RiskSolutionResponseDTO>(existing);
        }

        public async Task<RiskSolutionResponseDTO?> UpdateMitigationPlanAsync(int id, string mitigationPlan, int createdBy)
        {
            if (string.IsNullOrWhiteSpace(mitigationPlan))
                throw new ArgumentException("MitigationPlan must be provided.");
            if (createdBy <= 0)
                throw new ArgumentException("CreatedBy must be a positive integer.");

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            var risk = await _riskRepo.GetByIdAsync(existing.RiskId)
                ?? throw new Exception("Risk not found with provided RiskId.");

            existing.MitigationPlan = mitigationPlan;
            existing.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _repo.UpdateAsync(existing);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = risk.ProjectId,
                    RiskKey = risk.RiskKey,
                    RelatedEntityType = "Risk",
                    RelatedEntityId = risk.RiskKey,
                    ActionType = "UPDATE",
                    Message = $"Updated risk mitigation plan in risk '{risk.RiskKey}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change mitigation plan: {ex.Message}", ex);
            }

            return _mapper.Map<RiskSolutionResponseDTO>(existing);
        }

        public async Task DeleteRiskSolution(int id, int createdBy)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Risk solution with ID {id} not found.");

            var risk = await _riskRepo.GetByIdAsync(entity.RiskId)
                ?? throw new Exception("Risk not found with provided RiskId.");

            try
            {
                await _repo.Delete(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = risk.ProjectId,
                    RiskKey = risk.RiskKey,
                    RelatedEntityType = "Risk",
                    RelatedEntityId = risk.RiskKey,
                    ActionType = "DELETE",
                    Message = $"Deleted risk mitigation plan in risk '{risk.RiskKey}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete risk solution: {ex.Message}", ex);
            }
        }

        public async Task DeleteMitigationPlan(int id, int createdBy)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Risk solution with ID {id} not found.");
            var risk = await _riskRepo.GetByIdAsync(entity.RiskId)
            ?? throw new Exception("Risk not found with provided RiskId.");
            entity.MitigationPlan = null;
            entity.UpdatedAt = DateTime.UtcNow;
            bool deleteRow = entity.MitigationPlan == null && entity.ContingencyPlan == null;
            try
            {
                if (deleteRow)
                {
                    await _repo.Delete(entity);
                    await _activityLogService.LogAsync(new ActivityLog
                    {
                        ProjectId = risk.ProjectId,
                        RiskKey = risk.RiskKey,
                        RelatedEntityType = "Risk",
                        RelatedEntityId = risk.RiskKey,
                        ActionType = "DELETE",
                        Message = $"Deleted mitigation plan in risk '{risk.RiskKey}'",
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    await _repo.UpdateAsync(entity);
                    await _activityLogService.LogAsync(new ActivityLog
                    {
                        ProjectId = risk.ProjectId,
                        RiskKey = risk.RiskKey,
                        RelatedEntityType = "Risk",
                        RelatedEntityId = risk.RiskKey,
                        ActionType = "UPDATE",
                        Message = $"Deleted mitigation plan in risk '{risk.RiskKey}'",
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete mitigation plan: {ex.Message}", ex);
            }
        }

        public async Task DeleteContingencyPlan(int id, int createdBy)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Risk solution with ID {id} not found.");
            var risk = await _riskRepo.GetByIdAsync(entity.RiskId)
            ?? throw new Exception("Risk not found with provided RiskId.");
            entity.ContingencyPlan = null;
            entity.UpdatedAt = DateTime.UtcNow;
            bool deleteRow = entity.MitigationPlan == null && entity.ContingencyPlan == null;
            try
            {
                if (deleteRow)
                {
                    await _repo.Delete(entity);
                    await _activityLogService.LogAsync(new ActivityLog
                    {
                        ProjectId = risk.ProjectId,
                        RiskKey = risk.RiskKey,
                        RelatedEntityType = "Risk",
                        RelatedEntityId = risk.RiskKey,
                        ActionType = "DELETE",
                        Message = $"Deleted contingency plan in risk '{risk.RiskKey}'",
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    await _repo.UpdateAsync(entity);
                    await _activityLogService.LogAsync(new ActivityLog
                    {
                        ProjectId = risk.ProjectId,
                        RiskKey = risk.RiskKey,
                        RelatedEntityType = "Risk",
                        RelatedEntityId = risk.RiskKey,
                        ActionType = "UPDATE",
                        Message = $"Deleted contingency plan in risk '{risk.RiskKey}'",
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete contingency plan: {ex.Message}", ex);
            }
        }
    }
}
