using IntelliPM.Data.DTOs.Milestone.Request;
using IntelliPM.Data.DTOs.Milestone.Response;
using IntelliPM.Data.DTOs.Task.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.MilestoneServices
{
    public interface IMilestoneService
    {
        Task<List<MilestoneResponseDTO>> GetAllMilestones();
        Task<MilestoneResponseDTO> GetMilestoneById(int id);
        Task<List<MilestoneResponseDTO>> GetMilestoneByName(string name);
        Task<MilestoneResponseDTO> CreateMilestone(MilestoneRequestDTO request);
        Task<MilestoneResponseDTO> UpdateMilestone(int id, MilestoneRequestDTO request);
        Task DeleteMilestone(int id);
        Task<MilestoneResponseDTO> ChangeMilestoneStatus(int id, string status);
        Task<List<MilestoneResponseDTO>> GetMilestonesByProjectIdAsync(int projectId);
        Task<MilestoneResponseDTO> ChangeMilestoneSprint(string key, int sprintId);
        Task<MilestoneResponseDTO> CreateQuickMilestone(MilestoneQuickRequestDTO request);
    }
}
