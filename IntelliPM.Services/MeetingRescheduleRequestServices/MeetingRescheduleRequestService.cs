using AutoMapper;
using IntelliPM.Data.DTOs.MeetingRescheduleRequest.Request;
using IntelliPM.Data.DTOs.MeetingRescheduleRequest.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MeetingRescheduleRequestRepos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingRescheduleRequestServices
{
    public class MeetingRescheduleRequestService : IMeetingRescheduleRequestService
    {
        private readonly IMeetingRescheduleRequestRepository _repo;
        private readonly IMapper _mapper;

        public MeetingRescheduleRequestService(IMeetingRescheduleRequestRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }


        public async Task<MeetingRescheduleRequestResponseDTO> CreateAsync(MeetingRescheduleRequestDTO dto)
        {
            var existing = await _repo.GetPendingByMeetingAndRequesterAsync(dto.MeetingId, dto.RequesterId);
            if (existing != null)
            {
                throw new InvalidOperationException("A pending reschedule request already exists for this meeting and requester.");
            }

            var entity = _mapper.Map<MeetingRescheduleRequest>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            var saved = await _repo.AddAsync(entity);
            return _mapper.Map<MeetingRescheduleRequestResponseDTO>(saved);
        }


        public async Task<MeetingRescheduleRequestResponseDTO> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Reschedule request not found");
            return _mapper.Map<MeetingRescheduleRequestResponseDTO>(entity);
        }

        public async Task<List<MeetingRescheduleRequestResponseDTO>> GetAllAsync()
        {
            // Bạn cần bổ sung hàm GetAllAsync vào repository nếu chưa có
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<List<MeetingRescheduleRequestResponseDTO>>(entities);
        }

        public async Task<MeetingRescheduleRequestResponseDTO> UpdateAsync(int id, MeetingRescheduleRequestDTO dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Reschedule request not found");
            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(entity);
            return _mapper.Map<MeetingRescheduleRequestResponseDTO>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Reschedule request not found");
            await _repo.DeleteAsync(entity);
        }

        public async Task<List<MeetingRescheduleRequestResponseDTO>> GetByRequesterIdAsync(int requesterId)
        {
            var entities = await _repo.GetByRequesterIdAsync(requesterId);
            return _mapper.Map<List<MeetingRescheduleRequestResponseDTO>>(entities);
        }

    }
}