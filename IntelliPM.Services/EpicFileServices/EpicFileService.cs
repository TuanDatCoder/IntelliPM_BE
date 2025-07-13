using AutoMapper;
using IntelliPM.Data.DTOs.EpicFile.Request;
using IntelliPM.Data.DTOs.EpicFile.Response;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.EpicFileRepos;
using IntelliPM.Repositories.TaskFileRepos;
using IntelliPM.Services.CloudinaryStorageServices;
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
        private readonly IMapper _mapper;

        public EpicFileService(IEpicFileRepository repository, ICloudinaryStorageService cloudinaryService, IMapper mapper)
        {
            _repository = repository;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
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
            return _mapper.Map<EpicFileResponseDTO>(entity);
        }

        public async Task<bool> DeleteEpicFileAsync(int epicId)
        {
            var epicFile = await _repository.GetByIdAsync(epicId);
            if (epicFile == null) return false;

            await _repository.DeleteAsync(epicFile);
            return true;
        }

        public async Task<List<EpicFileResponseDTO>> GetFilesByEpicIdAsync(string epicId)
        {
            var files = await _repository.GetFilesByEpicIdAsync(epicId);
            return _mapper.Map<List<EpicFileResponseDTO>>(files);
        }

    }
}
