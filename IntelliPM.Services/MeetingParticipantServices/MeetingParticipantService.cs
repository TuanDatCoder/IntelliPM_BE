using AutoMapper;
using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.MeetingParticipant.Request;
using IntelliPM.Data.DTOs.MeetingParticipant.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.MeetingParticipant;
using IntelliPM.Repositories.MeetingParticipantRepos;
using IntelliPM.Repositories.MeetingRepos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingParticipantServices
{
    public class MeetingParticipantService : IMeetingParticipantService
    {
        private readonly IMeetingParticipantRepository _repo;
        private readonly IMapper _mapper;
        private readonly IMeetingRepository _meetingRepo;
        private readonly Su25Sep490IntelliPmContext _context;


        public MeetingParticipantService(IMeetingParticipantRepository repo,IMeetingRepository meetingRepo, IMapper mapper, Su25Sep490IntelliPmContext context)
        {
            _repo = repo;
            _meetingRepo = meetingRepo;
            _mapper = mapper;
            _context = context;
        }

        //public async Task<MeetingParticipantResponseDTO> CreateParticipant(MeetingParticipantRequestDTO dto)
        //{
        //    var participant = _mapper.Map<MeetingParticipant>(dto);
        //    participant.CreatedAt = DateTime.UtcNow;

        //    await _repo.AddAsync(participant);
        //    return _mapper.Map<MeetingParticipantResponseDTO>(participant);
        //}


        public async Task<MeetingParticipantResponseDTO> CreateParticipant(MeetingParticipantRequestDTO dto)
        {
            // Lấy thông tin meeting để lấy thời gian
            var meeting = await _meetingRepo.GetByIdAsync(dto.MeetingId);
            if (meeting == null)
                throw new Exception("Meeting not found.");

            // Đảm bảo StartTime và EndTime không null
            if (!meeting.StartTime.HasValue || !meeting.EndTime.HasValue)
                throw new Exception("Meeting start time or end time is missing.");

            // Kiểm tra trùng lịch
            bool hasConflict = await _repo.HasTimeConflictAsync(
                dto.AccountId, meeting.StartTime.Value, meeting.EndTime.Value, meeting.Id);
            if (hasConflict)
                throw new Exception("This participant has a conflicting meeting.");

            // Thêm participant như bình thường
            var participant = _mapper.Map<MeetingParticipant>(dto);
            participant.CreatedAt = DateTime.UtcNow;
            participant.Status = "invite";
            await _repo.AddAsync(participant);
            return _mapper.Map<MeetingParticipantResponseDTO>(participant);
        }


        public async Task<MeetingParticipantResponseDTO> GetParticipantById(int id)
        {
            var participant = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Participant not found.");
            return _mapper.Map<MeetingParticipantResponseDTO>(participant);
        }

        //public async Task<List<MeetingParticipantResponseDTO>> GetParticipantsByMeetingId(int meetingId)
        //{
        //    var participants = await _repo.GetByMeetingIdAsync(meetingId);
        //    return _mapper.Map<List<MeetingParticipantResponseDTO>>(participants);
        //}

        public async Task<List<MeetingParticipantResponseDTO>> GetParticipantsByMeetingId(int meetingId)
        {
            var participants = await _context.MeetingParticipant
                .Where(mp => mp.MeetingId == meetingId)
                .Join(_context.Account,
                      mp => mp.AccountId,
                      acc => acc.Id,
                      (mp, acc) => new MeetingParticipantResponseDTO
                      {
                          Id = mp.Id,
                          MeetingId = mp.MeetingId,
                          AccountId = mp.AccountId,
                          Role = mp.Role,
                          Status = mp.Status,
                          CreatedAt = mp.CreatedAt,
                          FullName = acc.FullName 
                      })
                .ToListAsync();

            return participants;
        }

        public async Task<MeetingParticipantResponseDTO> UpdateParticipant(int id, MeetingParticipantRequestDTO dto)
        {
            var participant = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Participant not found.");
            _mapper.Map(dto, participant);
            await _repo.UpdateAsync(participant);
            return _mapper.Map<MeetingParticipantResponseDTO>(participant);
        }

        public async Task<MeetingParticipantResponseDTO> UpdateParticipantStatus(int id, string newStatus)
        {
            var participant = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Participant not found.");
            participant.Status = newStatus; // "absent" hoặc "present"
            await _repo.UpdateAsync(participant);
            return _mapper.Map<MeetingParticipantResponseDTO>(participant);
        }

        public async Task DeleteParticipant(int id)
        {
            var participant = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Participant not found.");
            await _repo.DeleteAsync(participant);
        }

        public async Task CreateParticipantsForMeeting(int meetingId, int attendeesCount)
        {
            var participants = new List<MeetingParticipant>();
            for (int i = 0; i < attendeesCount; i++)
            {
                participants.Add(new MeetingParticipant
                {
                    MeetingId = meetingId,
                    AccountId = i + 1, // Example logic for AccountId
                    Role = "Attendee",
                    Status = MeetingParticipantStatusEnum.Active.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }

            foreach (var participant in participants)
            {
                await _repo.AddAsync(participant);
            }
        }
    }
}