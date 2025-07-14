using IntelliPM.Data.DTOs.MeetingRescheduleRequest.Request;
using IntelliPM.Data.DTOs.MeetingRescheduleRequest.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingRescheduleRequestServices
{
    public interface IMeetingRescheduleRequestService
    {
        Task<List<MeetingRescheduleRequestResponseDTO>> GetAllAsync();
        Task<MeetingRescheduleRequestResponseDTO> GetByIdAsync(int id);
        Task<MeetingRescheduleRequestResponseDTO> CreateAsync(MeetingRescheduleRequestDTO dto);
        Task<MeetingRescheduleRequestResponseDTO> UpdateAsync(int id, MeetingRescheduleRequestDTO dto);
        Task DeleteAsync(int id);
        Task<List<MeetingRescheduleRequestResponseDTO>> GetByRequesterIdAsync(int requesterId);

    }
}