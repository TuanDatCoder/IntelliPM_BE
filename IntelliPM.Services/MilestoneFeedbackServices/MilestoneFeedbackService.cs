using AutoMapper;
using IntelliPM.Data.DTOs.MilestoneFeedback.Request;
using IntelliPM.Data.DTOs.MilestoneFeedback.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.MeetingLog;
using IntelliPM.Repositories.MeetingLogRepos;
using IntelliPM.Repositories.MilestoneFeedbackRepos;
using Microsoft.Extensions.Logging;
using MFStatus = IntelliPM.Data.Enum.MilestoneFeedback.MilestoneFeedbackStatusEnum;


namespace IntelliPM.Services.MilestoneFeedbackServices
{
    public class MilestoneFeedbackService : IMilestoneFeedbackService
    {
        private readonly IMilestoneFeedbackRepository _feedbackRepo;
        private readonly IMeetingLogRepository _logRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<MilestoneFeedbackService> _logger;

        public MilestoneFeedbackService(
            IMilestoneFeedbackRepository feedbackRepo,
            IMeetingLogRepository logRepo,
            IMapper mapper,
            ILogger<MilestoneFeedbackService> logger)
        {
            _feedbackRepo = feedbackRepo;
            _logRepo = logRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<MilestoneFeedbackResponseDTO> SubmitFeedbackAsync(MilestoneFeedbackRequestDTO request)
        {
            // Lưu feedback vào bảng milestone_feedback
            var feedbackEntity = _mapper.Map<MilestoneFeedback>(request);
            feedbackEntity.CreatedAt = DateTime.UtcNow;

            await _feedbackRepo.AddAsync(feedbackEntity);

            // Ghi log vào bảng meeting_log
            var logEntity = new MeetingLog
            {
                MeetingId = request.MeetingId,
                AccountId = request.AccountId,
                Action = "FEEDBACK_RECEIVED" ?? MeetingLogTypeEnum.FEEDBACK_RECEIVED.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            await _logRepo.AddAsync(logEntity);

            _logger.LogInformation("Feedback submitted and log recorded for Meeting ID {MeetingId}", request.MeetingId);

            return _mapper.Map<MilestoneFeedbackResponseDTO>(feedbackEntity);
        }

        public async Task<MilestoneFeedbackResponseDTO> ApproveMilestoneAsync(int meetingId, int accountId)
        {
            // Lưu xác nhận vào bảng milestone_feedback
            var feedbackEntity = new MilestoneFeedback
            {
                MeetingId = meetingId,
                AccountId = accountId,
                FeedbackText = "Approved",
                Status = MFStatus.approved.ToString() ,
                CreatedAt = DateTime.UtcNow
            };

            await _feedbackRepo.AddAsync(feedbackEntity);

            // Ghi log vào bảng meeting_log
            var logEntity = new MeetingLog
            {
                MeetingId = meetingId,
                AccountId = accountId,
                Action = "MILESTONE_APPROVED" ?? MeetingLogTypeEnum.MILESTONE_APPROVED.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            await _logRepo.AddAsync(logEntity);

            _logger.LogInformation("Milestone approved and log recorded for Meeting ID {MeetingId}", meetingId);

            return _mapper.Map<MilestoneFeedbackResponseDTO>(feedbackEntity);
        }

        public async Task<MilestoneFeedbackResponseDTO?> GetFeedbackByMeetingIdAsync(int meetingId)
        {
            var feedback = await _feedbackRepo.GetByMeetingIdAsync(meetingId);
            if (feedback == null)
                return null;

            return _mapper.Map<MilestoneFeedbackResponseDTO>(feedback);
        }

        public async Task<MilestoneFeedbackResponseDTO> UpdateFeedbackAsync(int id, MilestoneFeedbackRequestDTO request)
        {
            var feedback = await _feedbackRepo.GetByIdAsync(id);
            if (feedback == null)
                throw new KeyNotFoundException($"Feedback with ID {id} not found.");

            feedback.FeedbackText = request.FeedbackText;
            feedback.Status = request.Status;
            feedback.MeetingId = request.MeetingId;
            feedback.AccountId = request.AccountId;

            await _feedbackRepo.UpdateAsync(feedback);

            return _mapper.Map<MilestoneFeedbackResponseDTO>(feedback);
        }

        public async Task<List<MilestoneFeedbackResponseDTO>> GetRejectedFeedbacksByMeetingIdAsync(int meetingId)
        {
            // gọi 2 lần theo 2 status khác nhau rồi gộp
            var list = new List<MilestoneFeedback>();

            var a = await _feedbackRepo.GetByMeetingIdAndStatusAsync(meetingId, "Reject");
            if (a != null) list.AddRange(a);

            var b = await _feedbackRepo.GetByMeetingIdAndStatusAsync(meetingId, "REJECTED");
            if (b != null) list.AddRange(b);

            if (list.Count == 0)
                return new List<MilestoneFeedbackResponseDTO>();

            // dedupe theo Id (phòng trùng)
            var merged = list
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .ToList();

            // Map + gán tên account
            return merged.Select(fb =>
            {
                var dto = _mapper.Map<MilestoneFeedbackResponseDTO>(fb);
                dto.AccountName = fb.Account?.FullName ?? "Unknown";
                return dto;
            }).ToList();
        }
        public async Task DeleteFeedbackAsync(int id)
        {
            var feedback = await _feedbackRepo.GetByIdAsync(id);
            if (feedback == null)
                throw new KeyNotFoundException($"Feedback with ID {id} not found.");

            await _feedbackRepo.DeleteAsync(feedback);
        }
    }
}