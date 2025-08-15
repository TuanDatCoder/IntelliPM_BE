using IntelliPM.Data.DTOs.Subtask.Response;
using IntelliPM.Data.DTOs.TaskAssignment.Request;
using IntelliPM.Data.DTOs.TaskAssignment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskAssignmentServices
{
    public interface ITaskAssignmentService
    {
        Task<List<TaskAssignmentResponseDTO>> GetAllAsync();
        Task<List<TaskAssignmentResponseDTO>> GetByTaskIdAsync(string taskId);
        Task<List<TaskAssignmentResponseDTO>> GetByAccountIdAsync(int accountId);
        Task<TaskAssignmentResponseDTO> GetByIdAsync(int id);
        Task<TaskAssignmentResponseDTO> CreateTaskAssignment(TaskAssignmentRequestDTO request);
        Task<TaskAssignmentResponseDTO> CreateTaskAssignmentQuick(string taskId, TaskAssignmentQuickRequestDTO request);
        Task<TaskAssignmentResponseDTO> UpdateTaskAssignment(int id, TaskAssignmentRequestDTO request);
        Task DeleteTaskAssignment(int id);
        Task<TaskAssignmentResponseDTO> ChangeStatus(int id, string status);
        Task<List<TaskAssignmentResponseDTO>> CreateListTaskAssignment(List<TaskAssignmentRequestDTO> requests);
        Task DeleteByTaskAndAccount(string taskId, int accountId);
        Task<List<TaskAssignmentHourDTO>> GetTaskAssignmentHoursByTaskIdAsync(string taskId);
        // Task<TaskAssignmentHourDTO> ChangeActualHours(int id, decimal hours);
        Task<bool> ChangeActualHoursAsync(List<TaskAssignmentHourRequestDTO> updates, int createdBy);
        Task<TaskAssignmentResponseDTO> ChangeAssignmentPlannedHours(int id, decimal plannedHours, int createdBy);
        Task<List<TaskAssignmentResponseDTO>> UpdateAssignmentPlannedHoursBulk(string taskId, List<(int AssignmentId, decimal PlannedHours)> updates, int createdBy);
    }
}
