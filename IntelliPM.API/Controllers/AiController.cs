using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.Ai.GenerateEpics.Request;
using IntelliPM.Data.DTOs.Ai.GenerateStoryTask.Request;
using IntelliPM.Data.DTOs.Ai.ProjectTaskPlanning.Request;
using IntelliPM.Data.DTOs.Ai.SprintTaskPlanning.Request;
using IntelliPM.Services.AiServices.GenerateEpicsServices;
using IntelliPM.Services.AiServices.SprintPlanningServices;
using IntelliPM.Services.AiServices.SprintTaskPlanningServices;
using IntelliPM.Services.AiServices.StoryTaskServices;
using IntelliPM.Services.AiServices.TaskPlanningServices;
using IntelliPM.Services.EpicServices;
using IntelliPM.Services.SubtaskServices;
using IntelliPM.Services.TaskServices;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ITaskService _taskService;
        private readonly IEpicService _epicService;
        private readonly ISprintPlanningService _sprintPlanningService;
        private readonly ISprintTaskPlanningService _sprintTaskPlanningService;
        private readonly IGenerateEpicService _generateEpicService;
        private readonly IGenerateStoryTaskService _generateStoryTaskService;

        public AiController(ITaskPlanningService taskPlanningService, ISubtaskService subtaskService, ISprintPlanningService sprintPlanningService, ITaskService taskService, ISprintTaskPlanningService sprintTaskPlanningService, IEpicService epicService, IGenerateEpicService generateEpicService, IGenerateStoryTaskService generateStoryTaskService)
        {
            _taskPlanningService = taskPlanningService ?? throw new ArgumentNullException(nameof(taskPlanningService));
            _subtaskService = subtaskService ?? throw new ArgumentNullException( nameof(subtaskService));
            _sprintPlanningService = sprintPlanningService ?? throw new ArgumentNullException(nameof(sprintPlanningService));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _epicService = epicService ?? throw new ArgumentNullException(nameof(epicService));
            _sprintTaskPlanningService = sprintTaskPlanningService ?? throw new ArgumentNullException(nameof(sprintTaskPlanningService));
            _generateEpicService = generateEpicService ?? throw new ArgumentNullException(nameof(generateEpicService));
            _generateStoryTaskService = generateStoryTaskService ?? throw new ArgumentNullException(nameof(_generateStoryTaskService));
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

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER")]
        [HttpPost("{taskId}/generate-subtask")]
        public async Task<IActionResult> GenerateSubtaskFromTaskTitle(string taskId)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new ApiResponseDTO { IsSuccess = false, Code = 401, Message = "Unauthorized" });
            }
            try
            {
                var subtask = await _subtaskService.GenerateSubtaskPreviewAsync(token, taskId);
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


        [HttpPost("{projectId}/generate-task")]
        public async Task<IActionResult> GenerateTaskFromProjectDescription(int projectId)
        {
            try
            {
                var task = await _taskService.GenerateTaskPreviewAsync(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task generated successfully (not saved)",
                    Data = task
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error generating Task: {ex.Message}"
                });
            }
        }

        [HttpPost("{projectId}/generate-epic")]
        public async Task<IActionResult> GenerateEpicFromProjectDescription(int projectId)
        {
            try
            {
                var task = await _epicService.GenerateEpicPreviewAsync(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Epic generated successfully (not saved)",
                    Data = task
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error generating Epic: {ex.Message}"
                });
            }
        }

        [HttpPost("{epicId}/generate-task-by-epic")]
        public async Task<IActionResult> GenerateTaskFromEpicDescription(string epicId)
        {
            try
            {
                var epic = await _taskService.GenerateTaskPreviewByEpicAsync(epicId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task generated successfully (not saved)",
                    Data = epic
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error generating Task: {ex.Message}"
                });
            }
        }


        [HttpPost("project/{id}/sprint-planning")]
        public async Task<IActionResult> GenerateSprintPlan(int id, [FromBody] SprintPlanningRequestDTO request)
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

            if (request.NumberOfSprints <= 0 || request.WeeksPerSprint <= 0)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "NumberOfSprints and WeeksPerSprint must be greater than 0"
                });
            }

            try
            {
                var plan = await _sprintPlanningService.GenerateSprintPlan(id, request.NumberOfSprints, request.WeeksPerSprint);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Sprint plan generated successfully",
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
                    Message = $"Error generating sprint plan: {ex.Message}"
                });
            }
        }

        [HttpPost("sprint/{sprintId}/generate-tasks")]
        public async Task<IActionResult> GenerateTasksForSprint(int sprintId, [FromBody] GenerateTasksForSprintRequestDTO request)
        {
            if (sprintId <= 0)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Sprint ID must be greater than 0"
                });
            }

            if (string.IsNullOrWhiteSpace(request.ProjectKey))
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Project key is required"
                });
            }

            try
            {
                var tasks = await _sprintTaskPlanningService.GenerateTasksForSprintAsync(sprintId, request.ProjectKey);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Tasks generated successfully (not saved)",
                    Data = tasks
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
                    Message = $"Error generating tasks: {ex.Message}"
                });
            }
        }


        [HttpPost("project/{id}/generate-epics")]
        public async Task<IActionResult> GenerateEpics(int id, [FromBody] GenerateEpicsRequestDTO request)
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
                var epics = await _generateEpicService.GenerateEpics(id, request.ExistingEpicTitles);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Epics generated successfully (not saved)",
                    Data = epics
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
                    Message = $"Error generating epics: {ex.Message}"
                });
            }
        }


        [HttpPost("project/{id}/generate-story-task")]
        public async Task<IActionResult> GenerateStoryOrTask(int id, [FromBody] GenerateStoryTaskRequestDTO request)
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
                var result = await _generateStoryTaskService.GenerateStoryOrTask(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = $"{request.Type} generated successfully (not saved)",
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = ex.Message
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
                    Message = $"Error generating {request.Type.ToLower()}: {ex.Message}"
                });
            }
        }




    }

   
}
