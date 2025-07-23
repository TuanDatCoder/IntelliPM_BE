using IntelliPM.Data.DTOs.SubtaskFile.Request;
using IntelliPM.Data.DTOs.SubtaskFile.Response;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.SubtaskFileServices
{
    public interface ISubtaskFileService
    {
        Task<SubtaskFileResponseDTO> UploadSubtaskFileAsync(SubtaskFileRequestDTO request);
        Task<bool> DeleteSubtaskFileAsync(int subtaskFileId, int createdBy);
        Task<List<SubtaskFileResponseDTO>> GetFilesBySubtaskIdAsync(string taskId);
    }
}
