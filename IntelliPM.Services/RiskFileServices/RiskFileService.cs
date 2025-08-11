using AutoMapper;
using Google.Cloud.Storage.V1;
using IntelliPM.Data.DTOs.RiskFile.Request;
using IntelliPM.Data.DTOs.RiskFile.Response;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.RiskFileRepos;
using IntelliPM.Repositories.RiskRepos;
using IntelliPM.Services.ActivityLogServices;
using IntelliPM.Services.CloudinaryStorageServices;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
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
        private readonly IActivityLogService _activityLogService;
        private readonly IRiskRepository _riskRepo;

        public RiskFileService(IRiskFileRepository repo, ICloudinaryStorageService cloudinaryService, IMapper mapper, IActivityLogService activityLogService, IRiskRepository riskRepo)
        {
            _repo = repo;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
            _activityLogService = activityLogService;
            _riskRepo = riskRepo;
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
                // Status = "UPLOADED"
            };

            var risk = await _riskRepo.GetByIdAsync(entity.RiskId);

            await _repo.AddAsync(entity);
            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = risk.ProjectId,
                RiskKey = risk.RiskKey,
                RelatedEntityType = "RiskFile",
                RelatedEntityId = risk.RiskKey,
                ActionType = "CREATE",
                Message = $"Upload file in risk '{risk.RiskKey}' is '{request.FileName}'",
                CreatedBy = request.UploadedBy,
                CreatedAt = DateTime.UtcNow
            });

            return _mapper.Map<RiskFileResponseDTO>(entity);
        }

        public async Task<bool> DeleteRiskFileAsync(int id, int createdBy)
        {
            var riskFile = await _repo.GetByIdAsync(id);
            if (riskFile == null) return false;

            var risk = await _riskRepo.GetByIdAsync(riskFile.RiskId);

            await _repo.DeleteAsync(riskFile);
            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = risk.ProjectId,
                RiskKey = risk.RiskKey,
                RelatedEntityType = "RiskFile",
                RelatedEntityId = risk.RiskKey,
                ActionType = "DELETE",
                Message = $"Deleted file '{riskFile.FileName}' in risk '{risk.RiskKey}'",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });
            return true;
        }

        public async Task<List<RiskFileResponseDTO>> GetByRiskIdAsync(int riskId)
        {
            var files = await _repo.GetByRiskIdAsync(riskId);
            return _mapper.Map<List<RiskFileResponseDTO>>(files);
        }

    }
}
