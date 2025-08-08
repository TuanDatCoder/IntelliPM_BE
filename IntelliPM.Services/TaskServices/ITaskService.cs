using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.Entities;
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
        Task<TaskUpdateResponseDTO> UpdateTaskTrue(string id, TaskUpdateRequestDTO request);
        Task<List<TaskResponseDTO>> CreateTasksBulkAsync(List<TaskRequestDTO> requests);
        Task DeleteTask(string id);
        Task<TaskResponseDTO> ChangeTaskStatus(string id, string status, int createdBy);
        Task<List<TaskResponseDTO>> GetTasksByProjectIdAsync(int projectId);
        Task<TaskResponseDTO> ChangeTaskType(string id, string type, int createdBy);
        Task<TaskResponseDTO> ChangeTaskTitle(string id, string title, int createdBy);
        Task<TaskResponseDTO> ChangeTaskPriority(string id, string priority, int createdBy);
        Task<TaskResponseDTO> ChangeTaskReporter(string id, int reporter, int createdBy);
        Task<TaskResponseDTO> ChangeTaskDescription(string id, string description, int createdBy);
        Task<TaskResponseDTO> ChangeTaskPlannedStartDate(string id, DateTime plannedStartDate, int createdBy);
        Task<TaskResponseDTO> ChangeTaskPlannedEndDate(string id, DateTime plannedEndDate, int createdBy);
        Task<TaskResponseDTO> ChangeTaskSprint(string id, int sprintId, int createdBy);
        Task<TaskResponseDTO> ChangeTaskEpic(string id, string epicId);
        Task<TaskDetailedResponseDTO> GetTaskByIdDetailed(string id);
        Task<List<TaskDetailedResponseDTO>> GetTasksByProjectIdDetailed(int projectId);
        Task<List<TaskResponseDTO>> GetTasksByEpicIdAsync(string epicId);
        //Task<TaskResponseDTO> CalculatePlannedHoursAsync(string id);
        //Task DistributePlannedHoursAsync(string taskId);
        Task<TaskResponseDTO> ChangeTaskPlannedHours(string id, decimal plannedHours);
        Task<TaskWithSubtaskDTO?> GetTaskWithSubtasksAsync(string id);
        Task<List<TaskBacklogResponseDTO>> GetBacklogTasksAsync(string projectKey);
        Task<List<TaskBacklogResponseDTO>> GetTasksBySprintIdAsync(int sprintId);
        Task<List<TaskBacklogResponseDTO>> GetTasksBySprintIdByStatusAsync(int sprintId, string status);
        Task<List<TaskBacklogResponseDTO>> GetTasksByAccountIdAsync(int accountId);
        Task<List<TaskResponseDTO>> GenerateTaskPreviewAsync(int projectId);

        Task<List<TaskResponseDTO>> GenerateTaskPreviewByEpicAsync(string epicId);
    }
}
