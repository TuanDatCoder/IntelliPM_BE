using AutoMapper;
using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.Meeting.Request;
using IntelliPM.Data.DTOs.Meeting.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MeetingParticipantRepos;
using IntelliPM.Repositories.MeetingRepos;
using IntelliPM.Services.EmailServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingServices
{
    public class MeetingService : IMeetingService
    {
        private readonly IMeetingRepository _repo;
        private readonly IMeetingParticipantRepository _meetingParticipantRepo;
        private readonly Su25Sep490IntelliPmContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public MeetingService(
            IMeetingRepository repo,
            IMeetingParticipantRepository meetingParticipantRepo,
            IMapper mapper,
            Su25Sep490IntelliPmContext context,
             IEmailService emailService
            )

        {
            _repo = repo;
            _meetingParticipantRepo = meetingParticipantRepo;
            _mapper = mapper;
            _context = context;
            _emailService = emailService;
        }

        public async Task<MeetingResponseDTO> CreateMeeting(MeetingRequestDTO dto)
        {
            try
            {
                // Kiểm tra nếu Project đã có họp vào cùng ngày
                bool hasConflict = await _context.Meeting.AnyAsync(m =>
                    m.ProjectId == dto.ProjectId &&
                    m.MeetingDate.Date == dto.MeetingDate.Date &&
                    m.Status != "CANCELLED"
                );

                if (hasConflict)
                {
                    throw new InvalidOperationException("Project already has a meeting scheduled on this date.");
                }

                // Validate danh sách người tham gia
                if (dto.ParticipantIds == null || !dto.ParticipantIds.Any())
                    throw new Exception("At least one participant is required.");

                // Kiểm tra trùng lịch cho từng participant
                if (dto.StartTime.HasValue && dto.EndTime.HasValue)
                {
                    foreach (var participantId in dto.ParticipantIds)
                    {
                        bool participantConflict = await _meetingParticipantRepo.HasTimeConflictAsync(
                            participantId, dto.StartTime.Value, dto.EndTime.Value);
                        if (participantConflict)
                            throw new Exception($"Participant {participantId} has a conflicting meeting.");
                    }
                }

                var meeting = _mapper.Map<Meeting>(dto);
                meeting.Status = "ACTIVE";
                meeting.CreatedAt = DateTime.UtcNow;

                // Bắt buộc xử lý UTC
                meeting.MeetingDate = DateTime.SpecifyKind(meeting.MeetingDate, DateTimeKind.Utc);
                if (meeting.StartTime.HasValue)
                    meeting.StartTime = DateTime.SpecifyKind(meeting.StartTime.Value, DateTimeKind.Utc);
                if (meeting.EndTime.HasValue)
                    meeting.EndTime = DateTime.SpecifyKind(meeting.EndTime.Value, DateTimeKind.Utc);

                await _repo.AddAsync(meeting);
                await _context.SaveChangesAsync();

                
                foreach (var participantId in dto.ParticipantIds)
                {
                    var account = await _context.Account.FindAsync(participantId);
                    if (account == null)
                    {
                        Console.WriteLine($"[EmailError] Account with id {participantId} not found.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(account.Email))
                    {
                        Console.WriteLine($"[EmailError] Account id {participantId} does not have a valid email.");
                    }
                    else
                    {
                        try
                        {
                            await _emailService.SendMeetingInvitation(
                                account.Email,
                                account.FullName ?? "User",
                                meeting.MeetingTopic,
                                meeting.StartTime ?? DateTime.UtcNow,
                                meeting.MeetingUrl ?? ""
                            );
                        }
                        catch (Exception emailEx)
                        {
                            Console.WriteLine($"[EmailError] Failed to send invitation to {account.Email}: {emailEx.Message}");
                        }
                    }

                    var participant = new MeetingParticipant
                    {
                        MeetingId = meeting.Id,
                        AccountId = participantId,
                        Role = "Attendee",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _meetingParticipantRepo.AddAsync(participant);
                }
                // Tạo MeetingLog cho người tạo (participant đầu tiên)
                if (dto.ParticipantIds != null && dto.ParticipantIds.Count > 0)
                {
                    var log = new MeetingLog
                    {
                        MeetingId = meeting.Id,
                        AccountId = dto.ParticipantIds[0],
                        Action = "CREATE_MEETING"
                    };
                    _context.MeetingLog.Add(log);
                    await _context.SaveChangesAsync();
                }

                return _mapper.Map<MeetingResponseDTO>(meeting);
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateMeeting: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
                throw new Exception("An error occurred while creating the meeting. Please try again.", ex);
            }
        }

        public async Task<MeetingResponseDTO> CreateInternalMeeting(MeetingRequestDTO dto)
        {
            try
            {
                // Không kiểm tra trùng lịch họp theo project trong ngày

                // Validate danh sách người tham gia
                if (dto.ParticipantIds == null || !dto.ParticipantIds.Any())
                    throw new Exception("At least one participant is required.");

                // Kiểm tra trùng lịch cho từng participant
                if (dto.StartTime.HasValue && dto.EndTime.HasValue)
                {
                    foreach (var participantId in dto.ParticipantIds)
                    {
                        bool participantConflict = await _meetingParticipantRepo.HasTimeConflictAsync(
                            participantId, dto.StartTime.Value, dto.EndTime.Value);
                        if (participantConflict)
                            throw new Exception($"Participant {participantId} has a conflicting meeting.");
                    }
                }

                var meeting = _mapper.Map<Meeting>(dto);
                meeting.Status = "ACTIVE";
                meeting.CreatedAt = DateTime.UtcNow;

                // Bắt buộc xử lý UTC
                meeting.MeetingDate = DateTime.SpecifyKind(meeting.MeetingDate, DateTimeKind.Utc);
                if (meeting.StartTime.HasValue)
                    meeting.StartTime = DateTime.SpecifyKind(meeting.StartTime.Value, DateTimeKind.Utc);
                if (meeting.EndTime.HasValue)
                    meeting.EndTime = DateTime.SpecifyKind(meeting.EndTime.Value, DateTimeKind.Utc);

                await _repo.AddAsync(meeting);
                await _context.SaveChangesAsync();

                // Thêm participant vào bảng MeetingParticipant
             
                foreach (var participantId in dto.ParticipantIds)
                {
                    var account = await _context.Account.FindAsync(participantId);
                    if (account == null)
                    {
                        Console.WriteLine($"[EmailError] Account with id {participantId} not found.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(account.Email))
                    {
                        Console.WriteLine($"[EmailError] Account id {participantId} does not have a valid email.");
                    }
                    else
                    {
                        try
                        {
                            await _emailService.SendMeetingInvitation(
                                account.Email,
                                account.FullName ?? "User",
                                meeting.MeetingTopic,
                                meeting.StartTime ?? DateTime.UtcNow,
                                meeting.MeetingUrl ?? ""
                            );
                        }
                        catch (Exception emailEx)
                        {
                            Console.WriteLine($"[EmailError] Failed to send invitation to {account.Email}: {emailEx.Message}");
                        }
                    }

                    var participant = new MeetingParticipant
                    {
                        MeetingId = meeting.Id,
                        AccountId = participantId,
                        Role = "Attendee",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _meetingParticipantRepo.AddAsync(participant);
                }
                // Tạo MeetingLog cho người tạo (participant đầu tiên)
                if (dto.ParticipantIds != null && dto.ParticipantIds.Count > 0)
                {
                    var log = new MeetingLog
                    {
                        MeetingId = meeting.Id,
                        AccountId = dto.ParticipantIds[0],
                        Action = "CREATE_MEETING"
                    };
                    _context.MeetingLog.Add(log);
                    await _context.SaveChangesAsync();
                }

                return _mapper.Map<MeetingResponseDTO>(meeting);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateInternalMeeting: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
                throw new Exception("An error occurred while creating the internal meeting. Please try again.", ex);
            }
        }



        public async Task<List<MeetingResponseDTO>> GetMeetingsByUser()
        {
            try
            {
                var meetings = await _repo.GetByAccountIdAsync(0);  // Lấy tất cả cuộc họp (có thể thay đổi theo logic của bạn)
                return _mapper.Map<List<MeetingResponseDTO>>(meetings);  // Chuyển đổi sang dạng DTO
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetMeetingsByUser: " + ex.Message);
                throw new Exception("An error occurred while retrieving meetings.");
            }
        }

        public async Task<List<MeetingResponseDTO>> GetMeetingsByAccount(int accountId)
        {
            return await _context.MeetingParticipant
                .Where(mp => mp.AccountId == accountId)
                .Select(mp => new MeetingResponseDTO
                {
                    Id = mp.Meeting.Id,
                    ProjectId = mp.Meeting.ProjectId,
                    MeetingTopic = mp.Meeting.MeetingTopic,
                    MeetingDate = mp.Meeting.MeetingDate,
                    StartTime = mp.Meeting.StartTime,
                    EndTime = mp.Meeting.EndTime,
                    Status = mp.Meeting.Status,
                    MeetingUrl = mp.Meeting.MeetingUrl,
                    Attendees = mp.Meeting.Attendees,
                    CreatedAt = mp.Meeting.CreatedAt
                    // Không cần map Project.Name hoặc IconUrl
                })
                .ToListAsync();
        }


        public async Task<MeetingResponseDTO> UpdateMeeting(int id, MeetingRequestDTO dto)
        {
            try
            {
                var meeting = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Meeting not found");
                _mapper.Map(dto, meeting);  // Ánh xạ và cập nhật cuộc họp
                await _repo.UpdateAsync(meeting);
                await _context.SaveChangesAsync();  // Lưu các thay đổi vào cơ sở dữ liệu
                return _mapper.Map<MeetingResponseDTO>(meeting);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateMeeting: " + ex.Message);
                throw new Exception("An error occurred while updating the meeting.");
            }
        }

        public async Task CancelMeeting(int id)
        {
            try
            {
                var meeting = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Meeting not found");
                meeting.Status = "CANCELLED";

                // Xóa tất cả participant liên quan
                var participants = await _context.MeetingParticipant
                    .Where(mp => mp.MeetingId == id)
                    .ToListAsync();

                _context.MeetingParticipant.RemoveRange(participants);

                await _repo.UpdateAsync(meeting);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CancelMeeting: " + ex.Message);
                throw new Exception("An error occurred while cancelling the meeting.");
            }
        }


        public async Task<List<MeetingResponseDTO>> GetManagedMeetingsByAccount(int accountId)
        {
            // Lấy các MeetingId mà account này đã tạo (Action = "CREATE_MEETING")
            var meetingIds = await _context.MeetingLog
                .Where(log => log.AccountId == accountId && log.Action == "CREATE_MEETING")
                .Select(log => log.MeetingId)
                .Distinct()
                .ToListAsync();

            // Lấy thông tin chi tiết các cuộc họp và map sang MeetingResponseDTO
            var meetings = await _context.Meeting
                .Where(m => meetingIds.Contains(m.Id))
                .ToListAsync();

            return _mapper.Map<List<MeetingResponseDTO>>(meetings);
        }
    }
}
