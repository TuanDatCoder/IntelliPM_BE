using AutoMapper;
using IntelliPM.Data.DTOs.Risk.Request;
using IntelliPM.Data.DTOs.Risk.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.RiskRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RiskServices
{
    public class RiskService : IRiskService
    {
        private readonly IRiskRepository _riskRepo;
        private readonly IMapper _mapper;

        public RiskService(IRiskRepository riskRepo, IMapper mapper)
        {
            _riskRepo = riskRepo;
            _mapper = mapper;
        }

        public async Task<List<RiskResponseDTO>> GetAllRisksAsync()
        {
            var risks = await _riskRepo.GetAllRisksAsync();
            return _mapper.Map<List<RiskResponseDTO>>(risks);
        }

        public async Task<List<RiskResponseDTO>> GetByProjectIdAsync(int projectId)
        {
            var risks = await _riskRepo.GetByProjectIdAsync(projectId);
            return _mapper.Map<List<RiskResponseDTO>>(risks);
        }

        public async Task<RiskResponseDTO> GetByIdAsync(int id)
        {
            var risk = await _riskRepo.GetByIdAsync(id)
                ?? throw new Exception("Risk not found");
            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task AddAsync(RiskRequestDTO request)
        {
            var risk = _mapper.Map<Risk>(request);
            risk.CreatedAt = DateTime.UtcNow;
            risk.UpdatedAt = DateTime.UtcNow;
            await _riskRepo.AddAsync(risk);
        }

        public async Task UpdateAsync(int id, RiskRequestDTO request)
        {
            var risk = await _riskRepo.GetByIdAsync(id)
                ?? throw new Exception("Risk not found");

            _mapper.Map(request, risk);
            risk.UpdatedAt = DateTime.UtcNow;

            await _riskRepo.UpdateAsync(risk);
        }

        public async Task DeleteAsync(int id)
        {
            var risk = await _riskRepo.GetByIdAsync(id)
                ?? throw new Exception("Risk not found");
            await _riskRepo.DeleteAsync(risk);
        }
    }

}
