using IntelliPM.Data.DTOs.EpicFile.Request;
using IntelliPM.Data.DTOs.EpicFile.Response;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.EpicFileServices
{
    public interface IEpicFileService
    {
        Task<EpicFileResponseDTO> UploadEpicFileAsync(EpicFileRequestDTO request);
        Task<bool> DeleteEpicFileAsync(int epicId, int createdBy);
        Task<List<EpicFileResponseDTO>> GetFilesByEpicIdAsync(string epicId);
    }
}
