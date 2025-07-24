using AutoMapper;
using IntelliPM.Data.DTOs.Risk.Response;
using IntelliPM.Data.DTOs.RiskSolution.Request;
using IntelliPM.Data.DTOs.RiskSolution.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.RiskRepos;
using IntelliPM.Repositories.RiskSolutionRepos;
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

        public RiskSolutionService(IRiskRepository riskRepo, IRiskSolutionRepository repo, IMapper mapper)
        {
            _riskRepo = riskRepo;
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<RiskSolutionResponseDTO>> GetByRiskIdAsync(int riskId)
        {
            var data = await _repo.GetByRiskIdAsync(riskId);
            return _mapper.Map<List<RiskSolutionResponseDTO>>(data);
        }

        public async Task<RiskSolutionResponseDTO> CreateAsync(RiskSolutionRequestDTO dto)
        {
            var entity = _mapper.Map<RiskSolution>(dto);
            entity.CreatedAt = entity.UpdatedAt = DateTime.UtcNow;

            await _repo.AddAsync(entity);
            return _mapper.Map<RiskSolutionResponseDTO>(entity);
        }

        public async Task<RiskSolutionResponseDTO?> UpdateContigencyPlanAsync(int id, string contigencyPlan)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            existing.ContingencyPlan = contigencyPlan;
            existing.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _repo.UpdateAsync(existing);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change contigency plan: {ex.Message}", ex);
            }

            return _mapper.Map<RiskSolutionResponseDTO>(existing);
        }

        public async Task<RiskSolutionResponseDTO?> UpdateMitigationPlanAsync(int id, string mitigationPlan)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            existing.MitigationPlan = mitigationPlan;
            existing.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _repo.UpdateAsync(existing);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change mitigation plan: {ex.Message}", ex);
            }

            return _mapper.Map<RiskSolutionResponseDTO>(existing);
        }

        public async Task DeleteRiskSolution(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Risk solution with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete risk solution: {ex.Message}", ex);
            }
        }
    }
}
