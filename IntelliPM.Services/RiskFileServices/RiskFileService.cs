using AutoMapper;
using IntelliPM.Data.DTOs.RiskFile.Request;
using IntelliPM.Data.DTOs.RiskFile.Response;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.RiskFileRepos;
using IntelliPM.Services.CloudinaryStorageServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RiskFileServices
{
    public class RiskFileService : IRiskFileService
    {
        private readonly IRiskFileRepository _repo;
        private readonly ICloudinaryStorageService _cloudinaryService;
        private readonly IMapper _mapper;

        public RiskFileService(IRiskFileRepository repo, ICloudinaryStorageService cloudinaryService, IMapper mapper)
        {
            _repo = repo;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
        }

        public async Task<RiskFileResponseDTO> UploadRiskFileAsync(RiskFileRequestDTO request)
        {
            var url = await _cloudinaryService.UploadFileAsync(request.File.OpenReadStream(), request.File.FileName);

            var entity = new RiskFile
            {
                RiskId = request.RiskId,
                FileName = request.FileName,
                FileUrl = url,
                UploadedBy = request.UploadedBy,
                //Status = "UPLOADED"
            };

            await _repo.AddAsync(entity);
            return _mapper.Map<RiskFileResponseDTO>(entity);
        }

        public async Task<bool> DeleteRiskFileAsync(int id)
        {
            var riskFile = await _repo.GetByIdAsync(id);
            if (riskFile == null) return false;

            await _repo.DeleteAsync(riskFile);
            return true;
        }

        public async Task<List<RiskFileResponseDTO>> GetByRiskIdAsync(int riskId)
        {
            var files = await _repo.GetByRiskIdAsync(riskId);
            return _mapper.Map<List<RiskFileResponseDTO>>(files);
        }

    }
}
