using IntelliPM.Data.DTOs.Subtask.Request;
using IntelliPM.Data.DTOs.Subtask.Response;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.SubtaskServices
{
    public interface ISubtaskService
    {
        Task<List<SubtaskResponseDTO>> GetAllSubtaskList();
        Task<SubtaskResponseDTO> GetSubtaskById(string id);
        Task<SubtaskFullResponseDTO> GetFullSubtaskById(string id);
        Task<SubtaskResponseDTO> CreateSubtask(SubtaskRequest1DTO request);
        Task<SubtaskResponseDTO> Create2Subtask(SubtaskRequest2DTO request);
        Task<SubtaskResponseDTO> UpdateSubtask(string id, SubtaskRequestDTO request);
        Task DeleteSubtask(string id);
        Task<List<SubtaskResponseDTO>> GetSubtaskByTaskIdAsync(string taskId);
        Task<List<Subtask>> GenerateSubtaskPreviewAsync(string taskId);
        Task<SubtaskResponseDTO> ChangeSubtaskStatus(string id, string status, int createdBy);
        Task<SubtaskDetailedResponseDTO> GetSubtaskByIdDetailed(string id);
        Task<List<SubtaskDetailedResponseDTO>> GetSubtaskByTaskIdDetailed(string taskId);
        Task<List<SubtaskDetailedResponseDTO>> GetSubtasksByProjectIdDetailed(int projectId);
        Task<List<SubtaskResponseDTO>> SaveGeneratedSubtasks(List<SubtaskRequest2DTO> previews);
        Task<SubtaskFullResponseDTO> ChangePlannedHours(string id, decimal hours);
        Task<List<SubtaskResponseDTO>> GetSubTaskByAccountId(int accountId);

    }
}
