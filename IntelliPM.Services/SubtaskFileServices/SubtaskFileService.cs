using AutoMapper;
using Google.Cloud.Storage.V1;
using IntelliPM.Data.DTOs.SubtaskFile.Request;
using IntelliPM.Data.DTOs.SubtaskFile.Response;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.ActivityLogActionType;
using IntelliPM.Data.Enum.ActivityLogRelatedEntityType;
using IntelliPM.Data.Enum.SubtaskFile;
using IntelliPM.Repositories.SubtaskFileRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskFileRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.ActivityLogServices;
using IntelliPM.Services.CloudinaryStorageServices;
using IntelliPM.Services.Helper.DynamicCategoryHelper;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.SubtaskFileServices
{
    public class SubtaskFileService : ISubtaskFileService
    {
        private readonly ISubtaskFileRepository _repository;
        private readonly ICloudinaryStorageService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;
        private readonly ITaskRepository _taskRepo;
        private readonly ISubtaskRepository _subtaskRepo;
        private readonly IDynamicCategoryHelper _dynamicCategoryHelper;

        public SubtaskFileService(ISubtaskFileRepository repository, ICloudinaryStorageService cloudinaryService, IMapper mapper, IActivityLogService activityLogService, ITaskRepository taskRepo, ISubtaskRepository subtaskRepo, IDynamicCategoryHelper dynamicCategoryHelper)
        {
            _repository = repository;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
            _activityLogService = activityLogService;
            _taskRepo = taskRepo;
            _subtaskRepo = subtaskRepo;
            _dynamicCategoryHelper = dynamicCategoryHelper;
        }
        public async Task<SubtaskFileResponseDTO> UploadSubtaskFileAsync(SubtaskFileRequestDTO request)
        {
            var url = await _cloudinaryService.UploadFileAsync(request.File.OpenReadStream(), request.File.FileName);

            var entity = new SubtaskFile
            {
                SubtaskId = request.SubtaskId,
                Title = request.Title,
                UrlFile = url,
                Status = SubtaskFileStatusEnum.UPLOADED.ToString(),
            };

            var subtask = await _subtaskRepo.GetByIdAsync(entity.SubtaskId);
            var projectId = subtask?.Task.ProjectId;

            await _repository.AddAsync(entity);
            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = projectId,
                TaskId = (await _subtaskRepo.GetByIdAsync(entity.SubtaskId))?.TaskId ?? null,
                SubtaskId = entity.SubtaskId,
                RelatedEntityType = ActivityLogRelatedEntityTypeEnum.SUBTASK_FILE.ToString(),
                RelatedEntityId = entity.SubtaskId,
                ActionType = ActivityLogActionTypeEnum.CREATE.ToString(),
                Message = $"Upload file in subtask '{entity.SubtaskId}' under task '{(await _subtaskRepo.GetByIdAsync(entity.SubtaskId))?.TaskId}'",
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            });

            return _mapper.Map<SubtaskFileResponseDTO>(entity);
        }

        public async Task<bool> DeleteSubtaskFileAsync(int fileId, int createdBy)
        {
            var subtaskFile = await _repository.GetByIdAsync(fileId);
            if (subtaskFile == null) return false;

            var subtask = await _subtaskRepo.GetByIdAsync(subtaskFile.SubtaskId);
            var projectId = subtask?.Task.ProjectId;

            await _repository.DeleteAsync(subtaskFile);
            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = projectId,
                TaskId = (await _subtaskRepo.GetByIdAsync(subtaskFile.SubtaskId))?.TaskId ?? null,
                SubtaskId = subtaskFile.SubtaskId,
                RelatedEntityType = ActivityLogRelatedEntityTypeEnum.SUBTASK_FILE.ToString(),
                RelatedEntityId = subtaskFile.SubtaskId,
                ActionType = ActivityLogActionTypeEnum.DELETE.ToString(),
                Message = $"Delete file in subtask '{subtaskFile.SubtaskId}' under task '{(await _subtaskRepo.GetByIdAsync(subtaskFile.SubtaskId))?.TaskId}'",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });
            return true;
        }

        public async Task<List<SubtaskFileResponseDTO>> GetFilesBySubtaskIdAsync(string subtaskId)
        {
            var files = await _repository.GetFilesBySubtaskIdAsync(subtaskId);
            return _mapper.Map<List<SubtaskFileResponseDTO>>(files);
        }

    }
}
