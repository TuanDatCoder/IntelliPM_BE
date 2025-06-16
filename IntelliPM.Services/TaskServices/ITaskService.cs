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
        Task<TaskResponseDTO> GetTaskById(int id);
        Task<List<TaskResponseDTO>> GetTaskByTitle(string title);
        Task<TaskResponseDTO> CreateTask(TaskRequestDTO request);
        Task<TaskResponseDTO> UpdateTask(int id, TaskRequestDTO request);
        Task DeleteTask(int id);
        Task<TaskResponseDTO> ChangeTaskStatus(int id, string status);
        Task<List<TaskResponseDTO>> GetTasksByProjectIdAsync(int projectId);
    }
}
