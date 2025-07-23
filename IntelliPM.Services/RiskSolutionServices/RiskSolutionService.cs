using AutoMapper;
using IntelliPM.Data.DTOs.RiskSolution.Request;
using IntelliPM.Data.DTOs.RiskSolution.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.RiskRepos;
using IntelliPM.Repositories.RiskSolutionRepos;
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

        public async Task<RiskSolutionResponseDTO> CreateAsync(RiskSolutionRequestDTO dto)
        {
            var entity = _mapper.Map<RiskSolution>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.AddAsync(entity);
            return _mapper.Map<RiskSolutionResponseDTO>(entity);
        }

        public async Task<List<RiskSolutionResponseDTO>> GetListByRiskIdAsync(int riskId)
        {
            var entities = await _repo.GetListByRiskIdAsync(riskId);
            return _mapper.Map<List<RiskSolutionResponseDTO>>(entities);
        }

        public async Task<RiskSolutionResponseDTO> UpdateAsync(int id, RiskSolutionRequestDTO request)
        {
            var existing = await _repo.GetByIdAsync(id); 
            if (existing == null)
            {
                throw new Exception($"RiskSolution with Id {id} not found.");
            }

            existing.MitigationPlan = request.MitigationPlan;
            existing.ContingencyPlan = request.ContingencyPlan;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);

            return _mapper.Map<RiskSolutionResponseDTO>(existing);
        }


    }
}
