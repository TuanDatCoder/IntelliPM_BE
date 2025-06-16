using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.DTOs.TaskComment.Request;
using IntelliPM.Data.DTOs.TaskComment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskCommentServices
{
    public interface ITaskCommentService
    {
        Task<List<TaskCommentResponseDTO>> GetAllTaskComment();
        Task<TaskCommentResponseDTO> GetTaskCommentById(int id);
        Task<TaskCommentResponseDTO> CreateTaskComment(TaskCommentRequestDTO request);
        Task<TaskCommentResponseDTO> UpdateTaskComment(int id, TaskCommentRequestDTO request);
        Task DeleteTaskComment(int id);
    }
}
