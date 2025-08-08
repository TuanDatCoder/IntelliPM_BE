 using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.Subtask.Request;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Services.TaskServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _service;

        public TaskController(ITaskService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllTasks();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all tasks successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var task = await _service.GetTaskById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Task retrieved successfully",
                    Data = task
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpGet("by-title")]
        public async Task<IActionResult> GetByTitle([FromQuery] string title)
        {
            try
            {
                var tasks = await _service.GetTaskByTitle(title);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Tasks retrieved successfully",
                    Data = tasks
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
        }

        [HttpGet("{id}/detailed")]
        public async Task<IActionResult> GetByIdDetailed(string id)
        {
            try
            {
                var task = await _service.GetTaskByIdDetailed(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Task detailed retrieved successfully",
                    Data = task
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpGet("by-project-id/{projectId}/detailed")]
        public async Task<IActionResult> GetByProjectIdDetailed(int projectId)
        {
            try
            {
                var tasks = await _service.GetTasksByProjectIdDetailed(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = $"Tasks for Project ID {projectId} retrieved successfully",
                    Data = tasks
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateTask(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Task created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating task: {ex.Message}"
                });
            }
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<TaskRequestDTO> requests)
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Request list cannot be null or empty." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var results = await _service.CreateTasksBulkAsync(requests);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Tasks created successfully",
                    Data = results
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating tasks: {ex.Message}"
                });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] TaskRequestDTO request)
        {
            try
            {
                var updated = await _service.UpdateTask(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}/dat")]
        public async Task<IActionResult> UpdateTask(string id, [FromBody] TaskUpdateRequestDTO request)
        {
            try
            {
                var updated = await _service.UpdateTaskTrue(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _service.DeleteTask(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task deleted successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error deleting task: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeTaskStatusRequestDTO dto)
        {
            try
            {
                var updated = await _service.ChangeTaskStatus(id, dto.Status, dto.CreatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task status updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task status: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/sprint")]
        public async Task<IActionResult> ChangeSprint(string id, [FromBody] int sprintId)
        {
            try
            {
                var updated = await _service.ChangeTaskSprint(id, sprintId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task status updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task sprint: {ex.Message}"
                });
            }
        }


        [HttpPatch("{id}/epic")]
        public async Task<IActionResult> ChangeSprint(string id, [FromBody] string epicId)
        {
            try
            {
                var updated = await _service.ChangeTaskEpic(id, epicId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task status updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task epic: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/type")]
        public async Task<IActionResult> ChangeType(string id, [FromBody] ChangeTaskTypeRequestDTO dto)
        {
            try
            {
                var updated = await _service.ChangeTaskType(id, dto.Type, dto.CreatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task type updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task type: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/title")]
        public async Task<IActionResult> ChangeTitle(string id, [FromBody] ChangeTaskTitleRequestDTO dto)
        {
            try
            {
                var updated = await _service.ChangeTaskTitle(id, dto.Title, dto.CreatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task title updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task title: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/reporter")]
        public async Task<IActionResult> ChangeReporter(string id, [FromBody] ChangeTaskReporterRequestDTO dto)
        {
            try
            {
                var updated = await _service.ChangeTaskReporter(id, dto.ReporterId, dto.CreatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task reporter updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task reporter: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/description")]
        public async Task<IActionResult> ChangeDescription(string id, [FromBody] ChangeTaskDescriptionRequestDTO dto)
        {
            try
            {
                var updated = await _service.ChangeTaskDescription(id, dto.Description, dto.CreatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task description updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task description: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/priority")]
        public async Task<IActionResult> ChangePriority(string id, [FromBody] ChangeTaskPriorityRequestDTO dto)
        {
            try
            {
                var updated = await _service.ChangeTaskPriority(id, dto.Priority, dto.CreatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task priority updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task priority: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/planned-end-date")]
        public async Task<IActionResult> ChangePlannedEndDate(string id, [FromBody] ChangeTaskPlanEndDateRequestDTO dto)
        {
            try
            {
                var updated = await _service.ChangeTaskPlannedEndDate(id, dto.PlannedEndDate, dto.CreatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task plannedEndDate updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task plannedEndDate: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/planned-start-date")]
        public async Task<IActionResult> ChangePlannedStartDate(string id, [FromBody] ChangeTaskPlanStartDateRequestDTO dto)
        {
            try
            {
                var updated = await _service.ChangeTaskPlannedStartDate(id, dto.PlannedStartDate, dto.CreatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task plannedStartDate updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task plannedStartDate: {ex.Message}"
                });
            }
        }

        [HttpGet("by-project-id")]
        public async Task<IActionResult> GetByProjectId(int projectId)
        {
            try
            {
                var tasks = await _service.GetTasksByProjectIdAsync(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = $"Tasks for Project ID {projectId} retrieved successfully",
                    Data = tasks
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet("by-epic-id")]
        public async Task<IActionResult> GetByEpicId(string epicId)
        {
            try
            {
                var tasks = await _service.GetTasksByEpicIdAsync(epicId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = $"Tasks for Epic ID {epicId} retrieved successfully",
                    Data = tasks
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/planned-hours")]
        public async Task<IActionResult> ChangePlannedHours(string id, [FromBody] decimal hours)
        {
            try
            {
                var updated = await _service.ChangeTaskPlannedHours(id, hours);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task plannedHours updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating task plannedHours: {ex.Message}"
                });
            }
        }


        [HttpGet("with-subtasks")]
        public async Task<IActionResult> GetTaskWithSubtasks([FromQuery] string id)
        {
            try
            {
                var task = await _service.GetTaskWithSubtasksAsync(id);
                if (task == null)
                    return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = "Task not found" });
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task view successfully",
                    Data = task
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error fetching task with subtasks: {ex.Message}"
                });
            }
        }
         

        [HttpGet("backlog")]
        public async Task<IActionResult> GetBacklog([FromQuery] string projectKey)
        {
            try
            {
                var tasks = await _service.GetBacklogTasksAsync(projectKey);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Backlog tasks retrieved successfully",
                    Data = tasks
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,

                    Message = $"Error fetching task with subtasks: {ex.Message}"
                });
            }
        }


    

        [HttpGet("by-sprint-id/{sprintId}")]
        public async Task<IActionResult> GetBySprintId(int sprintId)
        {
            try
            {
                var tasks = await _service.GetTasksBySprintIdAsync(sprintId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = $"Tasks for Sprint ID {sprintId} retrieved successfully",
                    Data = tasks
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet("by-sprint-id/{sprintId}/task-status")]
        public async Task<IActionResult> GetBySpGetTasksBySprintIdAndStatus(int sprintId, [FromQuery] string taskStatus)
        {
            try
            {
                var tasks = await _service.GetTasksBySprintIdByStatusAsync(sprintId, taskStatus);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = $"Tasks for Sprint ID {sprintId} retrieved successfully",
                    Data = tasks
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }


        [HttpGet("by-account-id/{accountId}")]
        public async Task<IActionResult> GetByAccountId(int accountId)
        {
            try
            {
                var tasks = await _service.GetTasksByAccountIdAsync(accountId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = $"Tasks for Account ID {accountId} retrieved successfully",
                    Data = tasks
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }




    }
}