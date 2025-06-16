using AutoMapper;
using IntelliPM.Data.DTOs.MeetingParticipant.Request;
using IntelliPM.Data.DTOs.MeetingParticipant.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MeetingParticipantRepos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingParticipantServices
{
    public class MeetingParticipantService : IMeetingParticipantService
    {
        private readonly IMeetingParticipantRepository _repo;
        private readonly IMapper _mapper;

        public MeetingParticipantService(IMeetingParticipantRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<MeetingParticipantResponseDTO> CreateParticipant(MeetingParticipantRequestDTO dto)
        {
            var participant = _mapper.Map<MeetingParticipant>(dto);
            participant.CreatedAt = DateTime.UtcNow;

            await _repo.AddAsync(participant);
            return _mapper.Map<MeetingParticipantResponseDTO>(participant);
        }

        public async Task<MeetingParticipantResponseDTO> GetParticipantById(int id)
        {
            var participant = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Participant not found.");
            return _mapper.Map<MeetingParticipantResponseDTO>(participant);
        }

        public async Task<List<MeetingParticipantResponseDTO>> GetParticipantsByMeetingId(int meetingId)
        {
            var participants = await _repo.GetByMeetingIdAsync(meetingId);
            return _mapper.Map<List<MeetingParticipantResponseDTO>>(participants);
        }

        public async Task<MeetingParticipantResponseDTO> UpdateParticipant(int id, MeetingParticipantRequestDTO dto)
        {
            var participant = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Participant not found.");
            _mapper.Map(dto, participant);
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
                    Status = "Active",
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