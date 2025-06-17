using AutoMapper;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.TaskFileRepos;
using IntelliPM.Services.CloudinaryStorageServices;
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

        public TaskFileService(ITaskFileRepository repository, ICloudinaryStorageService cloudinaryService, IMapper mapper)
        {
            _repository = repository;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
        }

        public async Task<TaskFileResponseDTO> UploadTaskFileAsync(TaskFileRequestDTO request)
        {
            var url = await _cloudinaryService.UploadFileAsync(request.File.OpenReadStream(), request.File.FileName);

            var entity = new TaskFile
            {
                TaskId = request.TaskId,
                Title = request.Title,
                UrlFile = url,
                Status = "UPLOADED"
            };

            await _repository.AddAsync(entity);
            return _mapper.Map<TaskFileResponseDTO>(entity);
        }

        public async Task<bool> DeleteTaskFileAsync(int fileId)
        {
            var taskFile = await _repository.GetByIdAsync(fileId);
            if (taskFile == null) return false;

            await _repository.DeleteAsync(taskFile);
            return true;
        }

        public async Task<List<TaskFileResponseDTO>> GetFilesByTaskIdAsync(int taskId)
        {
            var files = await _repository.GetFilesByTaskIdAsync(taskId);
            return _mapper.Map<List<TaskFileResponseDTO>>(files);
        }

    }
}
