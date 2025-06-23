using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskFileServices
{
    public interface ITaskFileService
    {
        Task<TaskFileResponseDTO> UploadTaskFileAsync(TaskFileRequestDTO request);
        Task<bool> DeleteTaskFileAsync(int fileId);
        Task<List<TaskFileResponseDTO>> GetFilesByTaskIdAsync(string taskId);

    }
}
