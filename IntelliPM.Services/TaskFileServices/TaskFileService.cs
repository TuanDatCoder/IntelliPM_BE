using AutoMapper;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.ActivityLogActionType;
using IntelliPM.Data.Enum.ActivityLogRelatedEntityType;
using IntelliPM.Data.Enum.TaskFile;
using IntelliPM.Repositories.TaskFileRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.ActivityLogServices;
using IntelliPM.Services.CloudinaryStorageServices;
using IntelliPM.Services.Helper.DynamicCategoryHelper;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskFileServices
{
    public class TaskFileService : ITaskFileService
    {
        private readonly ITaskFileRepository _repository;
        private readonly ICloudinaryStorageService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;
        private readonly ITaskRepository _taskRepo;
        private readonly IDynamicCategoryHelper _dynamicCategoryHelper;

        public TaskFileService(ITaskFileRepository repository, ICloudinaryStorageService cloudinaryService, IMapper mapper, IActivityLogService activityLogService, ITaskRepository taskRepo, IDynamicCategoryHelper dynamicCategoryHelper)
        {
            _repository = repository;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
            _activityLogService = activityLogService;
            _taskRepo = taskRepo;
            _dynamicCategoryHelper = dynamicCategoryHelper;
        }

        public async Task<TaskFileResponseDTO> UploadTaskFileAsync(TaskFileRequestDTO request)
        {
            var url = await _cloudinaryService.UploadFileAsync(request.File.OpenReadStream(), request.File.FileName);

            var entity = new TaskFile
            {
                TaskId = request.TaskId,
                Title = request.Title,
                UrlFile = url,
                Status = TaskFileStatusEnum.UPLOADED.ToString(),
            };

            await _repository.AddAsync(entity);

            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = (await _taskRepo.GetByIdAsync(entity.TaskId))?.ProjectId ?? 0,
                TaskId = entity.TaskId,
                //SubtaskId = entity.Subtask,
                RelatedEntityType = ActivityLogRelatedEntityTypeEnum.TASK_FILE.ToString(),
                RelatedEntityId = entity.TaskId,
                ActionType = ActivityLogActionTypeEnum.CREATE.ToString(),
                Message = $"Upload file in task '{entity.TaskId}' is '{request.Title}'",
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            });

            return _mapper.Map<TaskFileResponseDTO>(entity);
        }

        public async Task<bool> DeleteTaskFileAsync(int fileId, int createdBy)
        {
            var taskFile = await _repository.GetByIdAsync(fileId);
            if (taskFile == null) return false;

            await _repository.DeleteAsync(taskFile);

            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = (await _taskRepo.GetByIdAsync(taskFile.TaskId))?.ProjectId ?? 0,
                TaskId = taskFile.TaskId,
                //SubtaskId = entity.Subtask,
                RelatedEntityType = ActivityLogRelatedEntityTypeEnum.TASK_FILE.ToString(),
                RelatedEntityId = taskFile.TaskId,
                ActionType = ActivityLogActionTypeEnum.DELETE.ToString(),
                Message = $"Delete file in task '{taskFile.TaskId}' is '{taskFile.Title}'",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });

            return true;
        }

        public async Task<List<TaskFileResponseDTO>> GetFilesByTaskIdAsync(string taskId)
        {
            var files = await _repository.GetFilesByTaskIdAsync(taskId);
            return _mapper.Map<List<TaskFileResponseDTO>>(files);
        }

    }
}
