//using AutoMapper;
//using IntelliPM.Data.DTOs.MeetingSummary.Request;
//using IntelliPM.Data.DTOs.MeetingSummary.Response;
//using IntelliPM.Data.Entities;
//using IntelliPM.Repositories.MeetingSummaryRepos;
//using System;
//using System.Threading.Tasks;

//namespace IntelliPM.Services.MeetingSummaryServices
//{
//    public class MeetingSummaryService : IMeetingSummaryService
//    {
//        private readonly IMeetingSummaryRepository _repo;
//        private readonly IMapper _mapper;

//        public MeetingSummaryService(IMeetingSummaryRepository repo, IMapper mapper)
//        {
//            _repo = repo;
//            _mapper = mapper;
//        }

//        public async Task<MeetingSummaryResponseDTO> CreateSummaryAsync(MeetingSummaryRequestDTO dto)
//        {
//            var entity = new MeetingSummary
//            {
//                MeetingTranscriptId = dto.MeetingTranscriptId,
//                SummaryText = dto.SummaryText,
//                CreatedAt = DateTime.UtcNow
//            };
//            var saved = await _repo.AddAsync(entity);
//            return _mapper.Map<MeetingSummaryResponseDTO>(saved);
//        }

//        public async Task<MeetingSummaryResponseDTO?> GetSummaryByTranscriptIdAsync(int meetingTranscriptId)
//        {
//            var entity = await _repo.GetByTranscriptIdAsync(meetingTranscriptId);
//            return entity == null ? null : _mapper.Map<MeetingSummaryResponseDTO>(entity);
//        }
//    }
//}

using AutoMapper;
using IntelliPM.Data.DTOs.MeetingSummary.Request;
using IntelliPM.Data.DTOs.MeetingSummary.Response;
using IntelliPM.Data.DTOs.MeetingTranscript.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MeetingSummaryRepos;
using IntelliPM.Repositories.MeetingTranscriptRepos;
using System;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingSummaryServices
{
    public class MeetingSummaryService : IMeetingSummaryService
    {
        private readonly IMeetingSummaryRepository _repo;
        private readonly IMeetingTranscriptRepository _transcriptRepo;
        private readonly IMapper _mapper;

        public async Task<MeetingSummaryResponseDTO> CreateSummaryAsync(MeetingSummaryRequestDTO dto)
        {
            var entity = new MeetingSummary
            {
                MeetingTranscriptId = dto.MeetingTranscriptId,
                SummaryText = dto.SummaryText,
                CreatedAt = DateTime.UtcNow
            };
            var saved = await _repo.AddAsync(entity);
            return _mapper.Map<MeetingSummaryResponseDTO>(saved);
        }

        public MeetingSummaryService(IMeetingSummaryRepository repo, IMeetingTranscriptRepository transcriptRepo, IMapper mapper)
        {
            _repo = repo;
            _transcriptRepo = transcriptRepo;
            _mapper = mapper;
        }

        public async Task<MeetingSummaryResponseDTO?> GetSummaryByTranscriptIdAsync(int meetingTranscriptId)
        {
            var summaryEntity = await _repo.GetByTranscriptIdAsync(meetingTranscriptId);
            if (summaryEntity == null) return null;

            var transcriptEntity = await _transcriptRepo.GetByMeetingIdAsync(meetingTranscriptId);
            var transcriptText = transcriptEntity?.TranscriptText;

            var response = _mapper.Map<MeetingSummaryResponseDTO>(summaryEntity);
            response.TranscriptText = transcriptText; // Add TranscriptText to the response
            return response;
        }
    }
}