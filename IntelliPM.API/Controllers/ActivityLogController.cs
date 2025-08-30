using Google.Api;
using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.ActivityLog.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Services.ActivityLogServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ActivityLogController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;
        private readonly Su25Sep490IntelliPmContext _context;

        public ActivityLogController(IActivityLogService activityLogService, Su25Sep490IntelliPmContext context)
        {
            _activityLogService = activityLogService;
            _context = context;
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER,ADMIN")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _activityLogService.GetAllActivityLogList();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all activitylog successfully",
                Data = result
            });
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER,ADMIN")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var activityLogList = await _activityLogService.GetActivityLogById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "ActivityLog retrieved successfully",
                    Data = activityLogList
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER,ADMIN")]
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetByProjectId(int projectId)
        {
            try
            {
                var activityLogList = await _activityLogService.GetActivityLogsByProjectId(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "ActivityLog retrieved successfully",
                    Data = activityLogList
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER,ADMIN")]
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetByTaskId(string taskId)
        {
            try
            {
                var activityLogList = await _activityLogService.GetActivityLogsByTaskId(taskId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "ActivityLog retrieved successfully",
                    Data = activityLogList
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER,ADMIN")]
        [HttpGet("subtask/{subtaskId}")]
        public async Task<IActionResult> GetBySubtaskId(string subtaskId)
        {
            try
            {
                var activityLogList = await _activityLogService.GetActivityLogsBySubtaskId(subtaskId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "ActivityLog retrieved successfully",
                    Data = activityLogList
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER,ADMIN")]
        [HttpGet("epic/{epicId}")]
        public async Task<IActionResult> GetByEpicId(string epicId)
        {
            try
            {
                var activityLogList = await _activityLogService.GetActivityLogsByEpicId(epicId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "ActivityLog retrieved successfully",
                    Data = activityLogList
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER,ADMIN")]
        [HttpGet]
        public IActionResult GetLogs([FromQuery] int projectId, [FromQuery] string? taskId, [FromQuery] string? subtaskId)
        {
            var query = _context.ActivityLog
                .Where(x => x.ProjectId == projectId);

            if (!string.IsNullOrEmpty(taskId))
                query = query.Where(x => x.TaskId == taskId);

            if (!string.IsNullOrEmpty(subtaskId))
                query = query.Where(x => x.SubtaskId == subtaskId);

            var result = query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ActivityLogResponseDTO
                {
                    Id = x.Id,
                    ProjectId = x.ProjectId,
                    TaskId = x.TaskId,
                    SubtaskId = x.SubtaskId,
                    RelatedEntityType = x.RelatedEntityType,
                    RelatedEntityId = x.RelatedEntityId,
                    ActionType = x.ActionType,
                    FieldChanged = x.FieldChanged,
                    OldValue = x.OldValue,
                    NewValue = x.NewValue,
                    Message = x.Message,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt
                })
                .ToList();

            return Ok(result);
        }

        //[Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER")]
        [HttpPost]
        public async Task<IActionResult> CreateLog([FromBody] ActivityLog log)
        {
            await _activityLogService.LogAsync(log);
            return Ok(new { message = "Log created successfully" });
        }

        [HttpGet("risk/{riskKey}")]
        public async Task<IActionResult> GetByRiskKey(string riskKey)
        {
            try
            {
                var activityLogList = await _activityLogService.GetActivityLogsByRiskKey(riskKey);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "ActivityLog retrieved successfully",
                    Data = activityLogList
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }
    }
}