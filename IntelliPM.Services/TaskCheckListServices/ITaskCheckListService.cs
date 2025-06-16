using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskCheckListServices
{
    public interface ITaskCheckListService
    {
        Task<List<TaskCheckListResponseDTO>> GetAllTaskCheckList();
        Task<TaskCheckListResponseDTO> GetTaskCheckListById(int id);
        Task<TaskCheckListResponseDTO> CreateTaskCheckList(TaskCheckListRequestDTO request);
        Task<TaskCheckListResponseDTO> UpdateTaskCheckList(int id, TaskCheckListRequestDTO request);
        Task DeleteTaskCheckList(int id);
    }
}
