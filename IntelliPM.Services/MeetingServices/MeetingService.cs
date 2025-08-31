using AutoMapper;
using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.Meeting.Request;
using IntelliPM.Data.DTOs.Meeting.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MeetingParticipantRepos;
using IntelliPM.Repositories.MeetingRepos;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.NotificationServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using MStatus = IntelliPM.Data.Enum.Meeting.MeetingStatusEnum;
using MPStatus = IntelliPM.Data.Enum.MeetingParticipant.MeetingParticipantStatusEnum;



namespace IntelliPM.Services.MeetingServices
{
    public class MeetingService : IMeetingService
    {
        private readonly IMeetingRepository _repo;
        private readonly IMeetingParticipantRepository _meetingParticipantRepo;
        private readonly Su25Sep490IntelliPmContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public MeetingService(
            IMeetingRepository repo,
            IMeetingParticipantRepository meetingParticipantRepo,
            IMapper mapper,
            Su25Sep490IntelliPmContext context,
             IEmailService emailService,
             INotificationService notificationService
            )

        {
            _repo = repo;
            _meetingParticipantRepo = meetingParticipantRepo;
            _mapper = mapper;
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<MeetingResponseDTO> CreateMeeting(MeetingRequestDTO dto)
        {
            try
            {
                // 1) Validate conflict theo Project + ngày
                bool hasConflict = await _context.Meeting.AnyAsync(m =>
                        m.ProjectId == dto.ProjectId &&
                        m.MeetingDate.Date == dto.MeetingDate.Date &&
                        m.Status != MStatus.CANCELLED.ToString()
                );
                if (hasConflict)
                    throw new InvalidOperationException("Project already has a meeting scheduled on this date.");

                // 2) Validate participant list
                if (dto.ParticipantIds == null || !dto.ParticipantIds.Any())
                    throw new Exception("At least one participant is required.");

                if (dto.StartTime.HasValue && dto.EndTime.HasValue)
                {
                    var start = dto.StartTime.Value;
                    var end = dto.EndTime.Value;

                    // Completed meeting cùng project, overlap thời gian
                    bool completedInProject = await _context.Meeting.AnyAsync(m =>
                            //m.ProjectId == dto.ProjectId &&
                            //m.Status == "COMPLETED" &&
                            //m.StartTime.HasValue && m.EndTime.HasValue &&
                            //m.StartTime.Value < end && m.EndTime.Value > start
                            m.ProjectId == dto.ProjectId &&
    m.Status == MStatus.COMPLETED.ToString()// <- đổi ở đây
    && m.StartTime.HasValue && m.EndTime.HasValue
    && m.StartTime.Value < end && m.EndTime.Value > start
                    );

                    if (completedInProject)
                        throw new Exception("A completed meeting already exists in this time range for the project.");

                  
                }

                // 3) Check trùng lịch từng participant (nếu có start/end)
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

                // 4) Map + chuẩn hóa UTC
                var meeting = _mapper.Map<Meeting>(dto);
                meeting.Status = MStatus.ACTIVE.ToString();
                meeting.CreatedAt = DateTime.UtcNow;

                meeting.MeetingDate = DateTime.SpecifyKind(meeting.MeetingDate, DateTimeKind.Utc);
                if (meeting.StartTime.HasValue)
                    meeting.StartTime = DateTime.SpecifyKind(meeting.StartTime.Value, DateTimeKind.Utc);
                if (meeting.EndTime.HasValue)
                    meeting.EndTime = DateTime.SpecifyKind(meeting.EndTime.Value, DateTimeKind.Utc);

                // 5) Save meeting
                await _repo.AddAsync(meeting);
                await _context.SaveChangesAsync();

                // 6) Lấy accounts hợp lệ trước (để dùng cho cả add participant & gửi email)
                var accounts = new List<(int Id, string? Email, string? FullName, string? Role)>();
                foreach (var pid in dto.ParticipantIds)
                {
                    var acc = await _context.Account.FindAsync(pid);
                    if (acc == null)
                    {
                        Console.WriteLine($"[EmailError] Account with id {pid} not found.");
                        continue;
                    }
                    accounts.Add((pid, acc.Email, acc.FullName, acc.Role));
                }

                // 7) Add participants (giữ nguyên logic repo)
                foreach (var acc in accounts)
                {
                    var participant = new MeetingParticipant
                    {
                        MeetingId = meeting.Id,
                        AccountId = acc.Id,
                        Role = acc.Role ?? "Attendee",
                        Status = MPStatus.Active.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _meetingParticipantRepo.AddAsync(participant);
                }

                // 8) Gửi email song song (bounded concurrency)
                var emailTasks = new List<Task>();
                var semaphore = new SemaphoreSlim(5); 

                foreach (var acc in accounts)
                {
                    if (string.IsNullOrWhiteSpace(acc.Email))
                    {
                        Console.WriteLine($"[EmailError] Account id {acc.Id} does not have a valid email.");
                        continue;
                    }

                    emailTasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await _emailService.SendMeetingInvitation(
                                acc.Email!,
                                acc.FullName ?? "User",
                                meeting.MeetingTopic,
                                meeting.StartTime ?? DateTime.UtcNow,
                                meeting.MeetingUrl ?? ""
                            );
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }

                await Task.WhenAll(emailTasks);

                // 9) MeetingLog cho người tạo (participant đầu tiên nếu có)
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

                // 10) Notification sau cùng
                await _notificationService.SendMeetingNotification(
                    dto.ParticipantIds,
                    meeting.Id,
                    meeting.MeetingTopic,
                    dto.ParticipantIds[0] // hoặc accountId đang tạo cuộc họp
                );

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
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                throw new Exception("An error occurred while creating the meeting. Please try again.", ex);
            }
        }
        public async Task<MeetingResponseDTO> CreateInternalMeeting(MeetingRequestDTO dto)
        {
            try
            {
                // Không check trùng lịch theo project trong ngày

                // 1) Validate participant list
                if (dto.ParticipantIds == null || !dto.ParticipantIds.Any())
                    throw new Exception("At least one participant is required.");

                // 2) Check trùng lịch từng participant (nếu có start/end)
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

                if (dto.StartTime.HasValue && dto.EndTime.HasValue)
                {
                    var start = dto.StartTime.Value;
                    var end = dto.EndTime.Value;

                    // Completed meeting cùng project, overlap thời gian
                    bool completedInProject = await _context.Meeting.AnyAsync(m =>
                        m.ProjectId == dto.ProjectId &&
                        m.Status != MStatus.COMPLETED.ToString() &&
                        m.StartTime.HasValue && m.EndTime.HasValue &&
                        m.StartTime.Value < end && m.EndTime.Value > start
                    );

                    if (completedInProject)
                        throw new Exception("A completed meeting already exists in this time range for the project.");


                }

                // 3) Map + chuẩn hóa UTC
                var meeting = _mapper.Map<Meeting>(dto);
                meeting.Status =  MStatus.ACTIVE.ToString();
                meeting.CreatedAt = DateTime.UtcNow;

                meeting.MeetingDate = DateTime.SpecifyKind(meeting.MeetingDate, DateTimeKind.Utc);
                if (meeting.StartTime.HasValue)
                    meeting.StartTime = DateTime.SpecifyKind(meeting.StartTime.Value, DateTimeKind.Utc);
                if (meeting.EndTime.HasValue)
                    meeting.EndTime = DateTime.SpecifyKind(meeting.EndTime.Value, DateTimeKind.Utc);

                // 4) Save meeting
                await _repo.AddAsync(meeting);
                await _context.SaveChangesAsync();

                // 5) Lấy accounts hợp lệ
                var accounts = new List<(int Id, string? Email, string? FullName, string? Role)>();
                foreach (var pid in dto.ParticipantIds)
                {
                    var acc = await _context.Account.FindAsync(pid);
                    if (acc == null)
                    {
                        Console.WriteLine($"[EmailError] Account with id {pid} not found.");
                        continue;
                    }
                    accounts.Add((pid, acc.Email, acc.FullName, acc.Role));
                }

                // 6) Add participants
                foreach (var acc in accounts)
                {
                    var participant = new MeetingParticipant
                    {
                        MeetingId = meeting.Id,
                        AccountId = acc.Id,
                        Role = acc.Role ?? "Attendee",
                        Status = MPStatus.Active.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _meetingParticipantRepo.AddAsync(participant);
                }

                // 7) Gửi email song song (bounded concurrency)
                var emailTasks = new List<Task>();
                var semaphore = new SemaphoreSlim(5); // <= giới hạn concurrency

                foreach (var acc in accounts)
                {
                    if (string.IsNullOrWhiteSpace(acc.Email))
                    {
                        Console.WriteLine($"[EmailError] Account id {acc.Id} does not have a valid email.");
                        continue;
                    }

                    emailTasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await _emailService.SendMeetingInvitation(
                                acc.Email!,
                                acc.FullName ?? "User",
                                meeting.MeetingTopic,
                                meeting.StartTime ?? DateTime.UtcNow,
                                meeting.MeetingUrl ?? ""
                            );
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }

                await Task.WhenAll(emailTasks);

                // 8) MeetingLog cho người tạo (participant đầu tiên nếu có)
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

                // 9) Gửi notification
                await _notificationService.SendMeetingNotification(
                    dto.ParticipantIds,
                    meeting.Id,
                    meeting.MeetingTopic,
                    dto.ParticipantIds[0]
                );

                return _mapper.Map<MeetingResponseDTO>(meeting);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateInternalMeeting: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                throw new Exception("An error occurred while creating the internal meeting. Please try again.", ex);
            }
        }

        public async Task<List<MeetingResponseDTO>> GetMeetingsByUser()
        {
            try
            {
                var meetings = await _repo.GetByAccountIdAsync(0);  
                return _mapper.Map<List<MeetingResponseDTO>>(meetings);  
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
                // 1) Lấy meeting hiện tại
                var meeting = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Meeting not found");

                // 2) Snapshot giá trị cũ để detect thay đổi
                var oldTopic = meeting.MeetingTopic;
                var oldDate = meeting.MeetingDate;
                var oldStart = meeting.StartTime;
                var oldEnd = meeting.EndTime;
                var oldUrl = meeting.MeetingUrl;
                var oldStatus = meeting.Status;

                // 3) Ánh xạ thay đổi từ DTO vào entity (không lưu vội)
                _mapper.Map(dto, meeting);

                // 4) Chuẩn hoá UTC cho các field thời gian
                meeting.MeetingDate = DateTime.SpecifyKind(meeting.MeetingDate, DateTimeKind.Utc);
                if (meeting.StartTime.HasValue)
                    meeting.StartTime = DateTime.SpecifyKind(meeting.StartTime.Value, DateTimeKind.Utc);
                if (meeting.EndTime.HasValue)
                    meeting.EndTime = DateTime.SpecifyKind(meeting.EndTime.Value, DateTimeKind.Utc);

                // 5) Detect thay đổi
                bool topicChanged = !string.Equals(oldTopic, meeting.MeetingTopic, StringComparison.Ordinal);
                bool dateChanged = oldDate.Date != meeting.MeetingDate.Date;
                bool startChanged = oldStart != meeting.StartTime;
                bool endChanged = oldEnd != meeting.EndTime;
                bool urlChanged = !string.Equals(oldUrl, meeting.MeetingUrl, StringComparison.Ordinal);
                bool statusChanged = !string.Equals(oldStatus, meeting.Status, StringComparison.Ordinal);

                bool anyImportantChange = topicChanged || dateChanged || startChanged || endChanged || urlChanged || statusChanged;

                // 6) Lưu thay đổi meeting
                await _repo.UpdateAsync(meeting);
                await _context.SaveChangesAsync();

                // 7) Nếu có thay đổi đáng kể -> gửi email + notification
                if (anyImportantChange)
                {
                    // Lấy tất cả participants (kèm info email/name/role)
                    var participants = await _context.MeetingParticipant
                        .Where(mp => mp.MeetingId == meeting.Id && mp.Status != "Removed")
                        .Join(_context.Account,
                            mp => mp.AccountId,
                            a => a.Id,
                            (mp, a) => new { a.Id, a.Email, a.FullName })
                        .AsNoTracking()
                        .ToListAsync();

                    // Build summary thay đổi (để nhét vào email body)
                    var changedLines = new List<string>();
                    if (topicChanged) changedLines.Add($"• Topic: <b>{oldTopic}</b> → <b>{meeting.MeetingTopic}</b>");
                    if (dateChanged) changedLines.Add($"• Date: <b>{oldDate:dd/MM/yyyy}</b> → <b>{meeting.MeetingDate:dd/MM/yyyy}</b>");
                    if (startChanged) changedLines.Add($"• Start: <b>{oldStart:HH:mm dd/MM/yyyy}</b> → <b>{meeting.StartTime:HH:mm dd/MM/yyyy}</b>");
                    if (endChanged) changedLines.Add($"• End: <b>{oldEnd:HH:mm dd/MM/yyyy}</b> → <b>{meeting.EndTime:HH:mm dd/MM/yyyy}</b>");
                    if (urlChanged) changedLines.Add($"• URL: <b>{oldUrl}</b> → <b>{meeting.MeetingUrl}</b>");
                    if (statusChanged) changedLines.Add($"• Status: <b>{oldStatus}</b> → <b>{meeting.Status}</b>");
                    var changeSummaryHtml = string.Join("<br/>", changedLines);

                    // 7.1 Gửi email song song (bounded concurrency)
                    var sem = new SemaphoreSlim(5);
                    var emailTasks = participants
                        .Where(p => !string.IsNullOrWhiteSpace(p.Email))
                        .Select(p => Task.Run(async () =>
                        {
                            await sem.WaitAsync();
                            try
                            {
                                await _emailService.SendMeetingUpdateEmail(
                                    p.Email!,
                                    p.FullName ?? "User",
                                    meeting.MeetingTopic,
                                    meeting.StartTime ?? DateTime.UtcNow,
                                    meeting.MeetingUrl ?? string.Empty,
                                    changeSummaryHtml
                                );
                            }
                            catch (Exception emailEx)
                            {
                                Console.WriteLine($"[EmailError] Update email -> {p.Email}: {emailEx.Message}");
                            }
                            finally { sem.Release(); }
                        })).ToList();

                    await Task.WhenAll(emailTasks);

                    // 7.2 Bắn notification (tái sử dụng hàm hiện có)
                    var participantIds = participants.Select(x => x.Id).ToList();
                    if (participantIds.Count > 0)
                    {
                        // ActorId: tuỳ bạn, có thể lấy từ context hoặc người cập nhật
                        var actorId = participantIds[0];
                        await _notificationService.SendMeetingNotification(
                            participantIds, meeting.Id, $"{meeting.MeetingTopic} (Updated)", actorId
                        );
                    }
                }

                return _mapper.Map<MeetingResponseDTO>(meeting);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateMeeting: " + ex.Message);
                throw new Exception("An error occurred while updating the meeting.");
            }
        }

        public async Task CompleteMeeting(int meetingId)
        {
            try
            {
                var meeting = await _repo.GetByIdAsync(meetingId) ?? throw new KeyNotFoundException("Meeting not found");
                meeting.Status = MStatus.COMPLETED.ToString();

                await _repo.UpdateAsync(meeting);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CompleteMeeting: " + ex.Message);
                throw new Exception("An error occurred while completing the meeting.");
            }
        }

        public async Task CancelMeeting(int id)
        {
            try
            {
                // Early exit nếu đã CANCELLED (tránh làm việc thừa)
                var status = await _context.Meeting
                    .Where(m => m.Id == id)
                    .Select(m => m.Status)
                    .FirstOrDefaultAsync();

                if (status == null)
                    throw new KeyNotFoundException("Meeting not found");
                if (status ==  MStatus.CANCELLED.ToString())
                    return;

                // Lấy info cần cho email (nhẹ, không track)
                var meetingInfo = await _context.Meeting
                    .Where(m => m.Id == id)
                    .Select(m => new { m.MeetingTopic, m.StartTime, m.MeetingUrl })
                    .AsNoTracking()
                    .FirstAsync();

                // Lấy targets 1 phát bằng JOIN (tránh FindAsync từng account)
                var targets = await _context.MeetingParticipant
                    .Where(mp => mp.MeetingId == id)
                    .Join(_context.Account,
                          mp => mp.AccountId,
                          a => a.Id,
                          (mp, a) => new { a.Email, a.FullName })
                    .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                    .AsNoTracking()
                    .ToListAsync();

                // Bulk UPDATE trạng thái meeting
                await _context.Meeting
                    .Where(m => m.Id == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(m => m.Status,  MStatus.CANCELLED.ToString()));

                // Bulk DELETE participants
                await _context.MeetingParticipant
                    .Where(mp => mp.MeetingId == id)
                    .ExecuteDeleteAsync();

                // Gửi email hủy SONG SONG (bounded concurrency để không choke SMTP)
                var sem = new SemaphoreSlim(5); // <= chỉnh tuỳ SMTP (2–10)
                var tasks = targets.Select(t => Task.Run(async () =>
                {
                    await sem.WaitAsync();
                    try
                    {
                        await _emailService.SendMeetingCancellationEmail(
                            t.Email!,
                            t.FullName ?? "User",
                            meetingInfo.MeetingTopic,
                            meetingInfo.StartTime ?? DateTime.UtcNow,
                            meetingInfo.MeetingUrl ?? ""
                        );
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"[EmailError] Failed to send cancellation to {t.Email}: {emailEx.Message}");
                    }
                    finally
                    {
                        sem.Release();
                    }
                })).ToList();

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CancelMeeting: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
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

        public async Task<List<int>> CheckMeetingConflictAsync(List<int> participantIds, DateTime date, DateTime startTime, DateTime endTime)
        {
            var validParticipantStatuses = new[] { "Active", "Present", "Absent" };
            var conflictingAccountIds = new List<int>();

            foreach (var accountId in participantIds)
            {
                // Find meetings for this account that overlap the requested time
                var hasConflict = await _context.MeetingParticipant
                    .Where(mp => mp.AccountId == accountId && validParticipantStatuses.Contains(mp.Status))
                    .Join(_context.Meeting,
                          mp => mp.MeetingId,
                          m => m.Id,
                          (mp, m) => m)
                    .AnyAsync(m =>
                        m.Status ==  MStatus.ACTIVE.ToString() &&
                        m.MeetingDate.Date == date.Date &&
                        m.StartTime < endTime &&
                        m.EndTime > startTime
                    );

                if (hasConflict)
                    conflictingAccountIds.Add(accountId);
            }

            return conflictingAccountIds;
        }

        public async Task<(List<int> Added, List<int> AlreadyIn, List<int> Conflicted, List<int> NotFound)>
        AddParticipantsAsync(int meetingId, List<int> participantIds)
        {
            if (participantIds == null || participantIds.Count == 0)
                throw new ArgumentException("Participant list cannot be empty.");

            var meeting = await _context.Meeting
                .Where(m => m.Id == meetingId)
                .Select(m => new { m.Id, m.Status, m.MeetingDate, m.StartTime, m.EndTime, m.MeetingTopic, m.MeetingUrl })
                .FirstOrDefaultAsync();

            if (meeting == null) throw new KeyNotFoundException("Meeting not found.");
            if (meeting.Status == MStatus.CANCELLED.ToString())
                throw new InvalidOperationException("Cannot add participants to a cancelled meeting.");

            // hiện có
            var existingIds = await _context.MeetingParticipant
                .Where(mp => mp.MeetingId == meetingId)
                .Select(mp => mp.AccountId)
                .ToListAsync();

            var reqSet = participantIds.Distinct().ToHashSet();
            var alreadyIn = reqSet.Intersect(existingIds).ToList();
            var needToAdd = reqSet.Except(existingIds).ToList();

            var added = new List<int>();
            var conflicted = new List<int>();
            var notFound = new List<int>();

            if (needToAdd.Count == 0)
                return (added, alreadyIn, conflicted, notFound);

            // fetch accounts
            var accounts = await _context.Account
                .Where(a => needToAdd.Contains(a.Id))
                .Select(a => new { a.Id, a.Email, a.FullName, a.Role })
                .AsNoTracking()
                .ToListAsync();

            var foundIds = accounts.Select(a => a.Id).ToHashSet();
            notFound = needToAdd.Except(foundIds).ToList();

            // check conflict nếu có time
            var okToInsert = new List<(int Id, string? Email, string? FullName, string? Role)>();
            if (meeting.StartTime.HasValue && meeting.EndTime.HasValue)
            {
                var start = meeting.StartTime.Value;
                var end = meeting.EndTime.Value;
                var validStatuses = new[] { "Active", "Present", "Absent" };

                foreach (var acc in accounts)
                {
                    var hasConflict = await _context.MeetingParticipant
                        .Where(mp => mp.AccountId == acc.Id && validStatuses.Contains(mp.Status))
                        .Join(_context.Meeting, mp => mp.MeetingId, m => m.Id, (mp, m) => m)
                        .AnyAsync(m =>
                            m.Status ==  MStatus.ACTIVE.ToString() &&
                            m.MeetingDate.Date == meeting.MeetingDate.Date &&
                            m.StartTime < end && m.EndTime > start
                        );

                    if (hasConflict) conflicted.Add(acc.Id);
                    else okToInsert.Add((acc.Id, acc.Email, acc.FullName, acc.Role));
                }
            }
            else
            {
                okToInsert = accounts.Select(a => (a.Id, a.Email, a.FullName, a.Role)).ToList();
            }

            // insert
            if (okToInsert.Any())
            {
                var entities = okToInsert.Select(acc => new MeetingParticipant
                {
                    MeetingId = meetingId,
                    AccountId = acc.Id,
                    Role = acc.Role ?? "Attendee",
                    Status = MPStatus.Active.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.MeetingParticipant.AddRangeAsync(entities);
                await _context.SaveChangesAsync();
                added = okToInsert.Select(x => x.Id).ToList();
            }

            // email song song
            if (okToInsert.Any())
            {
                var sem = new SemaphoreSlim(5);
                var tasks = okToInsert.Where(a => !string.IsNullOrWhiteSpace(a.Email)).Select(a => Task.Run(async () =>
                {
                    await sem.WaitAsync();
                    try
                    {
                        await _emailService.SendMeetingInvitation(
                            a.Email!, a.FullName ?? "User",
                            meeting.MeetingTopic,
                            meeting.StartTime ?? DateTime.UtcNow,
                            meeting.MeetingUrl ?? ""
                        );
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"[EmailError] Failed to send invitation to {a.Email}: {emailEx.Message}");
                    }
                    finally { sem.Release(); }
                })).ToList();
                await Task.WhenAll(tasks);
            }

            // notification cho người mới
            if (added.Count > 0)
            {
                await _notificationService.SendMeetingNotification(
                    added, meetingId, meeting.MeetingTopic, added.First()
                );
            }

            return (added, alreadyIn, conflicted, notFound);
        }

        public async Task<(bool Removed, string? Reason)> RemoveParticipantAsync(int meetingId, int accountId)
        {
            // 1) check meeting tồn tại + trạng thái
            var meeting = await _context.Meeting
                .Where(m => m.Id == meetingId)
                .Select(m => new { m.Id, m.Status, m.MeetingTopic, m.MeetingDate, m.StartTime, m.MeetingUrl })
                .FirstOrDefaultAsync();

            if (meeting == null) return (false, "Meeting not found");
            if (meeting.Status == MStatus.CANCELLED.ToString()) return (false, "Meeting is CANCELLED");

            // 2) không cho remove người tạo
            var isCreator = await _context.MeetingLog
                .AnyAsync(l => l.MeetingId == meetingId && l.AccountId == accountId && l.Action == "CREATE_MEETING");
            if (isCreator) return (false, "Cannot remove creator");

            // 3) check participant có trong meeting
            var mp = await _context.MeetingParticipant
                .FirstOrDefaultAsync(x => x.MeetingId == meetingId && x.AccountId == accountId);
            if (mp == null) return (false, "Participant not in meeting");

            // 4) Hard delete
            _context.MeetingParticipant.Remove(mp);
            await _context.SaveChangesAsync();

            // 5) Lấy info account để email/notify
            var acc = await _context.Account
                .Where(a => a.Id == accountId)
                .Select(a => new { a.Id, a.Email, a.FullName })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (acc != null && !string.IsNullOrWhiteSpace(acc.Email))
            {
                try
                {
                    await _emailService.SendMeetingRemovalEmail(
                        acc.Email,
                        acc.FullName ?? "User",
                        meeting.MeetingTopic,
                        meeting.StartTime ?? meeting.MeetingDate, // fallback
                        meeting.MeetingUrl ?? string.Empty
                    );
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"[EmailError] Removal -> {acc.Email}: {emailEx.Message}");
                }
            }

            try
            {
                await _notificationService.SendMeetingNotification(
                    new List<int> { accountId },
                    meetingId,
                    $"{meeting.MeetingTopic} (You have been removed)",
                    accountId
                );
            }
            catch (Exception notiEx)
            {
                Console.WriteLine($"[NotifyError] Removal notify -> {accountId}: {notiEx.Message}");
            }

            return (true, null);
        }


    }
}
