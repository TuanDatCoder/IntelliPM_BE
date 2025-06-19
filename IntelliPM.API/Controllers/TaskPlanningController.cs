using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.Ai.ProjectTaskPlanning.Request;
using IntelliPM.Services.AiServices.TaskPlanningServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskPlanningController : ControllerBase
    {
        private readonly ITaskPlanningService _taskPlanningService;

        public TaskPlanningController(ITaskPlanningService taskPlanningService)
        {
            _taskPlanningService = taskPlanningService ?? throw new ArgumentNullException(nameof(taskPlanningService));
        }

        [HttpPost("generate-plan")]
        public async Task<IActionResult> GenerateTaskPlan([FromBody] ProjectTaskPlanningRequestDTO request)
        {
            if (request.ProjectId <= 0)
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
                var plan = await _taskPlanningService.GenerateTaskPlan(request);
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

    }
}