﻿using AutoMapper;
using IntelliPM.Data.DTOs.MilestoneFeedback.Request;
using IntelliPM.Data.DTOs.MilestoneFeedback.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MilestoneFeedbackRepos;
using IntelliPM.Repositories.MeetingLogRepos;
using Microsoft.Extensions.Logging;

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
                Action = "FEEDBACK_RECEIVED",
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
                Status = "approved",
                CreatedAt = DateTime.UtcNow
            };

            await _feedbackRepo.AddAsync(feedbackEntity);

            // Ghi log vào bảng meeting_log
            var logEntity = new MeetingLog
            {
                MeetingId = meetingId,
                AccountId = accountId,
                Action = "MILESTONE_APPROVED",
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
            var feedbacks = await _feedbackRepo.GetByMeetingIdAndStatusAsync(meetingId, "Reject");
            if (feedbacks == null || feedbacks.Count == 0)
                return new List<MilestoneFeedbackResponseDTO>();

            // Map và gán tên account
            return feedbacks.Select(fb =>
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