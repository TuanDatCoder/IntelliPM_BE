using IntelliPM.Data.DTOs.TaskDependency.Request;
using IntelliPM.Data.DTOs.TaskDependency.Response;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskDependencyServices
{
    public interface ITaskDependencyService
    {
        Task<TaskDependencyResponseDTO> CreateAsync(TaskDependencyRequestDTO dto);
        Task<List<TaskDependencyResponseDTO>> GetByLinkedFromAsync(string linkedFrom);
        Task<List<TaskDependencyResponseDTO>> CreateManyAsync(List<TaskDependencyIdRequestDTO> dtos);
    }
}
