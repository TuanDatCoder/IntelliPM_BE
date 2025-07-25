﻿using IntelliPM.Data.DTOs.Task.Request;
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
        Task<TaskUpdateResponseDTO> UpdateTaskTrue(string id, TaskUpdateRequestDTO request);
        Task DeleteTask(string id);
        Task<TaskResponseDTO> ChangeTaskStatus(string id, string status);
        Task<List<TaskResponseDTO>> GetTasksByProjectIdAsync(int projectId);
        Task<TaskResponseDTO> ChangeTaskType(string id, string type);
        Task<TaskResponseDTO> ChangeTaskTitle(string id, string title);
        Task<TaskResponseDTO> ChangeTaskDescription(string id, string description);
        Task<TaskResponseDTO> ChangeTaskPlannedStartDate(string id, DateTime plannedStartDate);
        Task<TaskResponseDTO> ChangeTaskPlannedEndDate(string id, DateTime plannedEndDate);
        Task<TaskResponseDTO> ChangeTaskSprint(string id, int sprintId);
        Task<TaskDetailedResponseDTO> GetTaskByIdDetailed(string id);
        Task<List<TaskDetailedResponseDTO>> GetTasksByProjectIdDetailed(int projectId);
        Task<List<TaskResponseDTO>> GetTasksByEpicIdAsync(string epicId);
        //Task<TaskResponseDTO> CalculatePlannedHoursAsync(string id);
        //Task DistributePlannedHoursAsync(string taskId);
        Task<TaskResponseDTO> ChangeTaskPlannedHours(string id, decimal plannedHours);
        Task<TaskWithSubtaskDTO?> GetTaskWithSubtasksAsync(string id);
        Task<List<TaskBacklogResponseDTO>> GetBacklogTasksAsync(string projectKey);
        Task<List<TaskBacklogResponseDTO>> GetTasksBySprintIdAsync(int sprintId);

    }
}
