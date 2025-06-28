using AutoMapper;
using IntelliPM.Data.DTOs.MeetingLog.Request;
using IntelliPM.Data.DTOs.MeetingLog.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MeetingLogRepos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingLogServices;

public class MeetingLogService : IMeetingLogService
{
    private readonly IMeetingLogRepository _repo;
    private readonly IMapper _mapper;

    public MeetingLogService(IMeetingLogRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<MeetingLogResponseDTO> AddLogAsync(MeetingLogRequestDTO dto)
    {
        var log = _mapper.Map<MeetingLog>(dto);
        log.CreatedAt = DateTime.UtcNow;

        var addedLog = await _repo.AddAsync(log);
        return _mapper.Map<MeetingLogResponseDTO>(addedLog);
    }

    public async Task<List<MeetingLogResponseDTO>> GetLogsByAccountIdAsync(int accountId)
    {
        var logs = await _repo.GetByAccountIdAsync(accountId);
        return logs.Select(log => new MeetingLogResponseDTO
        {
            Id = log.Id,
            MeetingId = log.MeetingId,
            AccountId = log.AccountId,
            Action = log.Action,
            CreatedAt = log.CreatedAt,
            AccountName = log.Account.FullName // Map AccountName từ Account.FullName
        }).ToList();
    }

    public async Task<List<MeetingLogResponseDTO>> GetLogsByMeetingIdAsync(int meetingId)
    {
        var logs = await _repo.GetByMeetingIdAsync(meetingId);
        return logs.Select(log => new MeetingLogResponseDTO
        {
            Id = log.Id,
            MeetingId = log.MeetingId,
            AccountId = log.AccountId,
            Action = log.Action,
            CreatedAt = log.CreatedAt,
            AccountName = log.Account.FullName // Map AccountName từ Account.FullName
        }).ToList();
    }

    public async Task<List<MeetingLogResponseDTO>> GetAllLogsAsync()
    {
        var logs = await _repo.GetAllAsync();
        return logs.Select(log => new MeetingLogResponseDTO
        {
            Id = log.Id,
            MeetingId = log.MeetingId,
            AccountId = log.AccountId,
            Action = log.Action,
            CreatedAt = log.CreatedAt,
            AccountName = log.Account.FullName // Map AccountName từ Account.FullName
        }).ToList();
    }

    //public async Task<List<MeetingLogResponseDTO>> GetLogsByMeetingIdAsync(int meetingId)
    //{
    //    var logs = await _epicRepo.GetByMeetingIdAsync(meetingId);
    //    return _mapper.Map<List<MeetingLogResponseDTO>>(logs);
    //}

    //public async Task<List<MeetingLogResponseDTO>> GetLogsByAccountIdAsync(int accountId)
    //{
    //    var logs = await _epicRepo.GetByAccountIdAsync(accountId);
    //    return _mapper.Map<List<MeetingLogResponseDTO>>(logs);
    //}

    //public async Task<List<MeetingLogResponseDTO>> GetAllLogsAsync()
    //{
    //    var logs = await _epicRepo.GetAllAsync();
    //    return _mapper.Map<List<MeetingLogResponseDTO>>(logs);
    //}
}