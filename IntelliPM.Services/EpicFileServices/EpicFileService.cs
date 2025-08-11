using AutoMapper;
using IntelliPM.Data.DTOs.EpicFile.Request;
using IntelliPM.Data.DTOs.EpicFile.Response;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.EpicFileRepos;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.TaskFileRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.ActivityLogServices;
using IntelliPM.Services.CloudinaryStorageServices;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.EpicFileServices
{
    public class EpicFileService : IEpicFileService
    {
        private readonly IEpicFileRepository _repository;
        private readonly ICloudinaryStorageService _cloudinaryService;
        private readonly IActivityLogService _activityLogService;
        private readonly IEpicRepository _epicRepo;
        private readonly IMapper _mapper;

        public EpicFileService(IEpicFileRepository repository, ICloudinaryStorageService cloudinaryService, IMapper mapper, IActivityLogService activityLogService, IEpicRepository epicRepo)
        {
            _repository = repository;
            _cloudinaryService = cloudinaryService;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _epicRepo = epicRepo;
            
        }

        public async Task<EpicFileResponseDTO> UploadEpicFileAsync(EpicFileRequestDTO request)
        {
            var url = await _cloudinaryService.UploadFileAsync(request.UrlFile.OpenReadStream(), request.UrlFile.FileName);

            var entity = new EpicFile
            {
                EpicId = request.EpicId,
                Title = request.Title,
                UrlFile = url,
                Status = "UPLOADED"
            };

            await _repository.AddAsync(entity);
            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = (await _epicRepo.GetByIdAsync(request.EpicId))?.ProjectId ?? 0,
                EpicId = request.EpicId,
                //TaskId = entity.TaskId,
                //SubtaskId = entity.Subtask,
                RelatedEntityType = "EpicFile",
                RelatedEntityId = request.EpicId,
                ActionType = "CREATE",
                Message = $"Upload file in epic '{request.EpicId}' is '{request.Title}'",
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            });
            return _mapper.Map<EpicFileResponseDTO>(entity);
        }

        public async Task<bool> DeleteEpicFileAsync(int epicId, int createdBy)
        {
            var epicFile = await _repository.GetByIdAsync(epicId);
            if (epicFile == null) return false;

            await _repository.DeleteAsync(epicFile);
            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = (await _epicRepo.GetByIdAsync(epicFile.EpicId))?.ProjectId ?? 0,
                EpicId = epicFile.EpicId,
                //TaskId = entity.TaskId,
                //SubtaskId = entity.Subtask,
                RelatedEntityType = "EpicFile",
                RelatedEntityId = epicFile.EpicId,
                ActionType = "DELETE",
                Message = $"Delete file in epic '{epicFile.EpicId}'",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });
            return true;
        }

        public async Task<List<EpicFileResponseDTO>> GetFilesByEpicIdAsync(string epicId)
        {
            var files = await _repository.GetFilesByEpicIdAsync(epicId);
            return _mapper.Map<List<EpicFileResponseDTO>>(files);
        }

    }
}
