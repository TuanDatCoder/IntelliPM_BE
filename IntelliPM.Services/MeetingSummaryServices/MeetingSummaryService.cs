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
using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.MeetingSummary.Request;
using IntelliPM.Data.DTOs.MeetingSummary.Response;
using IntelliPM.Data.DTOs.MeetingTranscript.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MeetingSummaryRepos;
using IntelliPM.Repositories.MeetingTranscriptRepos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingSummaryServices
{
    public class MeetingSummaryService : IMeetingSummaryService
    {
        private readonly IMeetingSummaryRepository _repo;
        private readonly IMeetingTranscriptRepository _transcriptRepo;
        private readonly IMapper _mapper;
        private readonly Su25Sep490IntelliPmContext _context;

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

        public MeetingSummaryService(IMeetingSummaryRepository repo, IMeetingTranscriptRepository transcriptRepo, IMapper mapper, Su25Sep490IntelliPmContext context)
        {
            _repo = repo;
            _transcriptRepo = transcriptRepo;
            _mapper = mapper;
            _context = context;
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

        public async Task<List<MeetingSummaryResponseDTO>?> GetSummariesByAccountIdAsync(int accountId)
        {
            var summaries = await _repo.GetByAccountIdAsync(accountId);
            if (summaries == null || summaries.Count == 0)
                return null;

            var result = new List<MeetingSummaryResponseDTO>();
            foreach (var summary in summaries)
            {
                var transcript = await _transcriptRepo.GetByMeetingIdAsync(summary.MeetingTranscriptId);
                var dto = _mapper.Map<MeetingSummaryResponseDTO>(summary);
                dto.TranscriptText = transcript?.TranscriptText;
                result.Add(dto);
            }
            return result;
        }

        public async Task<List<MeetingSummaryResponseDTO>> GetAllMeetingSummariesByAccountIdAsync(int accountId)
        {
            // Lấy tất cả meetingId mà account này tham gia
            var meetingIds = await _context.MeetingParticipant
                .Where(mp => mp.AccountId == accountId)
                .Select(mp => mp.MeetingId)
                .ToListAsync();

            // Lấy thông tin các cuộc họp
            var meetings = await _context.Meeting
                .Where(m => meetingIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.MeetingTopic);

            // Lấy tất cả transcript của các meeting này
            var transcripts = await _context.MeetingTranscript
                .Where(mt => meetingIds.Contains(mt.MeetingId))
                .ToListAsync();

            // Lấy tất cả summary của các transcript này
            var transcriptIds = transcripts.Select(t => t.MeetingId).ToList();
            var summaries = await _context.MeetingSummary
                .Where(ms => transcriptIds.Contains(ms.MeetingTranscriptId))
                .ToListAsync();

            // Lấy trạng thái milestone feedback
            var feedbacks = await _context.MilestoneFeedback
                .Where(fb => meetingIds.Contains(fb.MeetingId))
                .ToListAsync();

            var result = new List<MeetingSummaryResponseDTO>();

            foreach (var meetingId in meetingIds)
            {
                var transcript = transcripts.FirstOrDefault(t => t.MeetingId == meetingId);
                var summary = transcript != null
                    ? summaries.FirstOrDefault(s => s.MeetingTranscriptId == transcript.MeetingId)
                    : null;

                // Lấy trạng thái APPROVED
                var isApproved = feedbacks.Any(fb => fb.MeetingId == meetingId && fb.Status == "approved");

                var dto = new MeetingSummaryResponseDTO
                {
                    MeetingTranscriptId = transcript?.MeetingId ?? meetingId,
                    SummaryText = summary?.SummaryText ?? "Chờ cập nhật",
                    TranscriptText = transcript?.TranscriptText ?? "Chờ cập nhật",
                    CreatedAt = summary?.CreatedAt ?? DateTime.MinValue,
                    MeetingTopic = meetings.TryGetValue(meetingId, out var topic) ? topic : "Chờ cập nhật",
                    IsApproved = isApproved
                };
                result.Add(dto);
            }

            return result;
        }

        //public async Task<List<MeetingSummaryResponseDTO>> GetAllMeetingSummariesByAccountIdAsync(int accountId)
        //{
        //    // Lấy tất cả meetingId mà account này tham gia
        //    var meetingIds = await _context.MeetingParticipant
        //        .Where(mp => mp.AccountId == accountId)
        //        .Select(mp => mp.MeetingId)
        //        .ToListAsync();

        //    // Lấy tất cả transcript của các meeting này
        //    var transcripts = await _context.MeetingTranscript
        //        .Where(mt => meetingIds.Contains(mt.MeetingId))
        //        .ToListAsync();

        //    // Lấy tất cả summary của các transcript này
        //    var transcriptIds = transcripts.Select(t => t.MeetingId).ToList();
        //    var summaries = await _context.MeetingSummary
        //        .Where(ms => transcriptIds.Contains(ms.MeetingTranscriptId))
        //        .ToListAsync();

        //    var result = new List<MeetingSummaryResponseDTO>();

        //    foreach (var meetingId in meetingIds)
        //    {
        //        var transcript = transcripts.FirstOrDefault(t => t.MeetingId == meetingId);
        //        var summary = transcript != null
        //            ? summaries.FirstOrDefault(s => s.MeetingTranscriptId == transcript.MeetingId)
        //            : null;

        //        var dto = new MeetingSummaryResponseDTO
        //        {
        //            MeetingTranscriptId = transcript?.MeetingId ?? meetingId,
        //            SummaryText = summary?.SummaryText ?? "Chờ cập nhật",
        //            TranscriptText = transcript?.TranscriptText ?? "Chờ cập nhật",
        //            CreatedAt = summary?.CreatedAt ?? DateTime.MinValue
        //        };
        //        result.Add(dto);
        //    }

        //    return result;
        //}
    }
}