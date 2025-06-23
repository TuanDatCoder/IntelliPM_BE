using AutoMapper;
using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.Meeting.Request;
using IntelliPM.Data.DTOs.Meeting.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MeetingRepos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingServices
{
    public class MeetingService : IMeetingService
    {
        private readonly IMeetingRepository _repo;
        private readonly Su25Sep490IntelliPmContext _context;
        private readonly IMapper _mapper;

        public MeetingService(IMeetingRepository repo, IMapper mapper, Su25Sep490IntelliPmContext context)
        {
            _repo = repo;
            _mapper = mapper;
            _context = context;
        }

        //public async Task<MeetingResponseDTO> CreateMeeting(MeetingRequestDTO dto)
        //{
        //    try
        //    {
        //        // Kiểm tra ProjectId có tồn tại trong cơ sở dữ liệu không
        //        var project = await _context.Project.FindAsync(dto.ProjectId);
        //        if (project == null)
        //        {
        //            throw new KeyNotFoundException("Project not found.");
        //        }

        //        var meeting = _mapper.Map<Meeting>(dto);
        //        meeting.Status = "ACTIVE";
        //        meeting.CreatedAt = DateTime.UtcNow;

        //        // Lưu vào cơ sở dữ liệu
        //        await _repo.AddAsync(meeting);
        //        await _context.SaveChangesAsync();  // Lưu thay đổi vào cơ sở dữ liệu

        //        return _mapper.Map<MeetingResponseDTO>(meeting);  // Trả về MeetingResponseDTO
        //    }
        //    catch (Exception ex)
        //    {
        //        // Ghi chi tiết lỗi vào Console (hoặc sử dụng công cụ ghi log như Serilog, NLog)
        //        Console.WriteLine("Error in CreateMeeting: " + ex.Message);
        //        Console.WriteLine("Stack Trace: " + ex.StackTrace);
        //        if (ex.InnerException != null)
        //        {
        //            Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
        //        }

        //        // Trả về lỗi chi tiết cho người dùng
        //        throw new Exception("An error occurred while creating the meeting. Please try again.", ex);
        //    }
        //}

        public async Task<MeetingResponseDTO> CreateMeeting(MeetingRequestDTO dto)
        {
            try
            {
                // ⚠️ Check nếu Project đã có họp vào cùng ngày
                bool hasConflict = await _context.Meeting.AnyAsync(m =>
                    m.ProjectId == dto.ProjectId &&
                    m.MeetingDate.Date == dto.MeetingDate.Date &&
                    m.Status != "CANCELLED" // Optional: nếu không tính họp đã huỷ
                );

                if (hasConflict)
                {
                    throw new InvalidOperationException("Project already has a meeting scheduled on this date.");
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

                return _mapper.Map<MeetingResponseDTO>(meeting);
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi nghiệp vụ
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
                meeting.Status = "CANCELLED";  // Cập nhật trạng thái cuộc họp thành "CANCELLED"
                await _repo.UpdateAsync(meeting);
                await _context.SaveChangesAsync();  // Lưu các thay đổi vào cơ sở dữ liệu
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CancelMeeting: " + ex.Message);
                throw new Exception("An error occurred while cancelling the meeting.");
            }
        }
    }
}
