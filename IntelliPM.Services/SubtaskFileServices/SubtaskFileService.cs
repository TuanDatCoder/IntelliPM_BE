using AutoMapper;
using IntelliPM.Data.DTOs.SubtaskFile.Request;
using IntelliPM.Data.DTOs.SubtaskFile.Response;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.SubtaskFileRepos;
using IntelliPM.Repositories.TaskFileRepos;
using IntelliPM.Services.CloudinaryStorageServices;
using System;
using System.Collections.Generic;
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

        public SubtaskFileService(ISubtaskFileRepository repository, ICloudinaryStorageService cloudinaryService, IMapper mapper)
        {
            _repository = repository;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
        }

        public async Task<SubtaskFileResponseDTO> UploadSubtaskFileAsync(SubtaskFileRequestDTO request)
        {
            var url = await _cloudinaryService.UploadFileAsync(request.File.OpenReadStream(), request.File.FileName);

            var entity = new SubtaskFile
            {
                SubtaskId = request.SubtaskId,
                Title = request.Title,
                UrlFile = url,
                Status = "UPLOADED"
            };

            await _repository.AddAsync(entity);
            return _mapper.Map<SubtaskFileResponseDTO>(entity);
        }

        public async Task<bool> DeleteSubtaskFileAsync(int fileId)
        {
            var subtaskFile = await _repository.GetByIdAsync(fileId);
            if (subtaskFile == null) return false;

            await _repository.DeleteAsync(subtaskFile);
            return true;
        }

        public async Task<List<SubtaskFileResponseDTO>> GetFilesBySubtaskIdAsync(string subtaskId)
        {
            var files = await _repository.GetFilesBySubtaskIdAsync(subtaskId);
            return _mapper.Map<List<SubtaskFileResponseDTO>>(files);
        }

    }
}
