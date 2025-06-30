using IntelliPM.Data.DTOs.Task;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskServices
{
    public interface ITaskService
    {
        Task<List<TaskResponseDTO>> GetAllTasks();
        Task<TaskResponseDTO> GetTaskById(string id);
        Task<List<TaskResponseDTO>> GetTaskByTitle(string title);
        Task<TaskResponseDTO> CreateTask(TaskRequestDTO request);
        Task<TaskResponseDTO> UpdateTask(string id, TaskRequestDTO request);
        Task DeleteTask(string id);
        Task<TaskResponseDTO> ChangeTaskStatus(string id, string status);
        Task<List<TaskResponseDTO>> GetTasksByProjectIdAsync(int projectId);
        Task<TaskResponseDTO> ChangeTaskType(string id, string type);
        Task<TaskDetailedResponseDTO> GetTaskByIdDetailed(string id);
        Task<List<TaskDetailedResponseDTO>> GetTasksByProjectIdDetailed(int projectId);
    }
}
