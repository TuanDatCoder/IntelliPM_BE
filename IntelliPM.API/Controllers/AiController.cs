using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.Ai.ProjectTaskPlanning.Request;
using IntelliPM.Services.AiServices.TaskPlanningServices;
using IntelliPM.Services.TaskCheckListServices;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly ITaskPlanningService _taskPlanningService;
        private readonly ITaskCheckListService _taskCheckListService;

        public AiController(ITaskPlanningService taskPlanningService, ITaskCheckListService taskCheckListService)
        {
            _taskPlanningService = taskPlanningService ?? throw new ArgumentNullException(nameof(taskPlanningService));
            _taskCheckListService = taskCheckListService ?? throw new ArgumentNullException( nameof(taskCheckListService));
        }

        // Đạt: AI tạo gợi ý tạo các task cho project
        [HttpPost("project/{id}/task-planning")]
        public async Task<IActionResult> GenerateTaskPlan(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Project ID must be greater than 0"
                });
            }

            try
            {
                var plan = await _taskPlanningService.GenerateTaskPlan(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Task plan generated successfully",
                    Data = plan
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 404,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error generating task plan: {ex.Message}"
                });
            }
        }

        [HttpPost("{taskId}/generate-checklist")]
        public async Task<IActionResult> GenerateChecklistFromTitle(string taskId)
        {
            try
            {
                var checklist = await _taskCheckListService.GenerateChecklistPreviewAsync(taskId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Checklist generated successfully (not saved)",
                    Data = checklist
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error generating checklist: {ex.Message}"
                });
            }
        }

    }
}
