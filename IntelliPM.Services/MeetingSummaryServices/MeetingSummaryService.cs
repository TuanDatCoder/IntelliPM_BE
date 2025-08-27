

using AutoMapper;
using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.MeetingSummary.Request;
using IntelliPM.Data.DTOs.MeetingSummary.Response;
using IntelliPM.Data.DTOs.MeetingTranscript.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MeetingSummaryRepos;
using IntelliPM.Repositories.MeetingTranscriptRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace IntelliPM.Services.MeetingSummaryServices
{
    public class MeetingSummaryService : IMeetingSummaryService
    {
        private readonly IMeetingSummaryRepository _repo;
        private readonly IMeetingTranscriptRepository _transcriptRepo;
        private readonly IMapper _mapper;
        private readonly Su25Sep490IntelliPmContext _context;
        private readonly ILogger<MeetingSummaryService> _logger;
        private readonly string _historyRoot;

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

        public MeetingSummaryService(IMeetingSummaryRepository repo, IMeetingTranscriptRepository transcriptRepo, IMapper mapper, Su25Sep490IntelliPmContext context, ILogger<MeetingSummaryService> logger)
        {
            _repo = repo;
            _transcriptRepo = transcriptRepo;
            _mapper = mapper;
            _context = context;
            _logger = logger;
            _historyRoot = Path.Combine(AppContext.BaseDirectory, "summary_history");
            Directory.CreateDirectory(_historyRoot);
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
                .ToDictionaryAsync(m => m.Id, m => m);


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
                    SummaryText = summary?.SummaryText ?? "Wait for update",
                    TranscriptText = transcript?.TranscriptText ?? "Wait for update",
                    CreatedAt = meetings.TryGetValue(meetingId, out var meeting)
        ? meeting.MeetingDate
        : DateTime.MinValue,
                    MeetingTopic = meeting?.MeetingTopic ?? "Chờ cập nhật",
                    IsApproved = isApproved,
                      MeetingStatus = meeting?.Status ?? "Unknown"
                };

                result.Add(dto);
            }

            return result;
        }
        public async Task<bool> DeleteSummaryAndTranscriptAsync(int meetingTranscriptId)
        {
            var summary = await _context.MeetingSummary
                .FirstOrDefaultAsync(ms => ms.MeetingTranscriptId == meetingTranscriptId);

            var transcript = await _context.MeetingTranscript
                .FirstOrDefaultAsync(mt => mt.MeetingId == meetingTranscriptId);

            if (summary == null && transcript == null) return false;

            if (summary != null)
                _context.MeetingSummary.Remove(summary);

            if (transcript != null)
                _context.MeetingTranscript.Remove(transcript);

            await _context.SaveChangesAsync();
            return true;
        }
        private string GetHistoryDir(int meetingTranscriptId) => Path.Combine(_historyRoot, meetingTranscriptId.ToString());

        private static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input ?? string.Empty);
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        private async Task<string> SaveSnapshotAsync(MeetingSummary current, string? reason, int? editedBy)
        {
            var dir = GetHistoryDir(current.MeetingTranscriptId);
            Directory.CreateDirectory(dir);

            var payload = new
            {
                MeetingTranscriptId = current.MeetingTranscriptId,
                TakenAtUtc = DateTime.UtcNow,
                SummaryText = current.SummaryText,
                Hash = Sha256(current.SummaryText ?? string.Empty),
                CreatedAtOriginal = current.CreatedAt,
                EditReason = reason,
                EditedByAccountId = editedBy
            };

            var file = $"{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{payload.Hash}.json";
            var path = Path.Combine(dir, file);
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
            return path;
        }

        private async Task<string?> ReadSnapshotTextAsync(string path)
        {
            if (!File.Exists(path)) return null;
            using var fs = File.OpenRead(path);
            using var doc = await JsonDocument.ParseAsync(fs);
            return doc.RootElement.GetProperty("SummaryText").GetString();
        }

        // ADD: update + snapshot
        public async Task<MeetingSummaryResponseDTO> UpdateSummaryAsync(UpdateMeetingSummaryRequestDTO dto)
        {
            var current = await _repo.GetByTranscriptIdAsync(dto.MeetingTranscriptId);
            if (current == null)
                throw new Exception($"No summary found for meeting transcript ID {dto.MeetingTranscriptId}");

            if (!string.IsNullOrWhiteSpace(dto.IfMatchHash))
            {
                var serverHash = Sha256(current.SummaryText);
                if (!serverHash.Equals(dto.IfMatchHash, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("409_CONFLICT: Summary changed on server. Refresh and try again.");
            }

            await SaveSnapshotAsync(current, dto.EditReason, dto.EditedByAccountId); // snapshot before overwrite

            current.SummaryText = dto.SummaryText ?? string.Empty;
            await _repo.UpdateAsync(current);

            return _mapper.Map<MeetingSummaryResponseDTO>(current);
        }

        // ADD: list history
        public async Task<IEnumerable<SummaryHistoryItemDTO>> GetSummaryHistoryAsync(int meetingTranscriptId)
        {
            var dir = GetHistoryDir(meetingTranscriptId);
            if (!Directory.Exists(dir)) return Enumerable.Empty<SummaryHistoryItemDTO>();

            var files = Directory.EnumerateFiles(dir, "*.json", SearchOption.TopDirectoryOnly)
                .Select(p => new SummaryHistoryItemDTO
                {
                    FileName = Path.GetFileName(p),
                    TakenAtUtc = File.GetLastWriteTimeUtc(p)
                })
                .OrderByDescending(x => x.TakenAtUtc)
                .ToList();

            _logger.LogInformation("History {MeetingTranscriptId}: {Count} snapshots @ {Dir}", meetingTranscriptId, files.Count, dir);
            return await Task.FromResult(files);
        }

        // ADD: restore from a snapshot
        public async Task<MeetingSummaryResponseDTO> RestoreSummaryAsync(RestoreSummaryRequestDTO dto)
        {
            var dir = GetHistoryDir(dto.MeetingTranscriptId);
            var path = Path.Combine(dir, dto.FileName);
            var text = await ReadSnapshotTextAsync(path);
            if (text == null) throw new FileNotFoundException("Snapshot not found.");

            return await UpdateSummaryAsync(new UpdateMeetingSummaryRequestDTO
            {
                MeetingTranscriptId = dto.MeetingTranscriptId,
                SummaryText = text,
                EditReason = $"restore:{dto.FileName}" + (string.IsNullOrWhiteSpace(dto.Reason) ? "" : $" - {dto.Reason}"),
                EditedByAccountId = dto.EditedByAccountId
            });
        }

        public async Task<MeetingSummaryResponseDTO?> GetMeetingSummaryByMeetingIdAsync(int meetingId)
        {
            // Lấy thông tin meeting
            var meeting = await _context.Meeting
                .FirstOrDefaultAsync(m => m.Id == meetingId);
            if (meeting == null)
                return null;

            // Lấy transcript của meeting này
            var transcript = await _context.MeetingTranscript
                .FirstOrDefaultAsync(mt => mt.MeetingId == meetingId);

            // Lấy summary của transcript (nếu có)
            MeetingSummary? summary = null;
            if (transcript != null)
            {
                summary = await _context.MeetingSummary
                    .FirstOrDefaultAsync(ms => ms.MeetingTranscriptId == transcript.MeetingId);
            }

            // Lấy trạng thái milestone feedback
            var isApproved = await _context.MilestoneFeedback
                .AnyAsync(fb => fb.MeetingId == meetingId && fb.Status == "approved");

            // Tạo DTO trả về
            return new MeetingSummaryResponseDTO
            {
                MeetingTranscriptId = transcript?.MeetingId ?? 0,
                SummaryText = summary?.SummaryText ?? "Wait for update",
                TranscriptText = transcript?.TranscriptText ?? "Wait for update",
                CreatedAt = meeting.MeetingDate,
                MeetingTopic = meeting.MeetingTopic ?? "Chờ cập nhật",
                MeetingStatus = meeting.Status ?? "Unknown",
                IsApproved = isApproved
            };
        }


    }
}