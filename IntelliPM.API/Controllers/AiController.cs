﻿using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.Ai.ProjectTaskPlanning.Request;
using IntelliPM.Services.AiServices.TaskPlanningServices;
using IntelliPM.Services.SubtaskServices;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly ITaskPlanningService _taskPlanningService;
        private readonly ISubtaskService _subtaskService;


        public AiController(ITaskPlanningService taskPlanningService, ISubtaskService subtaskService)
        {
            _taskPlanningService = taskPlanningService ?? throw new ArgumentNullException(nameof(taskPlanningService));
            _subtaskService = subtaskService ?? throw new ArgumentNullException( nameof(subtaskService));
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

        [HttpPost("{taskId}/generate-subtask")]
        public async Task<IActionResult> GenerateSubtaskFromTaskTitle(string taskId)
        {
            try
            {
                var subtask = await _subtaskService.GenerateSubtaskPreviewAsync(taskId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Subtask generated successfully (not saved)",
                    Data = subtask
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error generating Subtask: {ex.Message}"
                });
            }
        }
    }
}
