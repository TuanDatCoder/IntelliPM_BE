using IntelliPM.Data.DTOs.Project.Request;
using IntelliPM.Data.DTOs;
using IntelliPM.Services.ProjectServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using IntelliPM.Data.Entities;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _service;

        public ProjectController(IProjectService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllProjects();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all projects successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var project = await _service.GetProjectById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Project retrieved successfully",
                    Data = project
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }


        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetProjectDetails(int id)
        {
            try
            {
                var projectDetails = await _service.GetProjectDetails(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Project details retrieved successfully",
                    Data = projectDetails
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
                    Message = $"Error retrieving project details: {ex.Message}"
                });
            }
        }






        [HttpGet("{id}/workitems")]
        public async Task<IActionResult> GetAllWorkItemsByProjectId(int id)
        {
            try
            {
                var workItems = await _service.GetAllWorkItemsByProjectId(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Work items retrieved successfully",
                    Data = workItems
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
                    Message = $"Error retrieving work items: {ex.Message}"
                });
            }
        }


        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? searchTerm, [FromQuery] string? projectType, [FromQuery] string? status)
        {
            try
            {
                var projects = await _service.SearchProjects(searchTerm ?? "", projectType, status);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Projects retrieved successfully",
                    Data = projects
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO { IsSuccess = false, Code = 500, Message = $"Error searching projects: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER")]
        public async Task<IActionResult> Create([FromBody] ProjectRequestDTO request)
        {

            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new ApiResponseDTO { IsSuccess = false, Code = 401, Message = "Unauthorized" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateProject(token,request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Project created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating project: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]

        public async Task<IActionResult> Update(int id, [FromBody] ProjectRequestDTO request)
        {
            try
            {
                var updated = await _service.UpdateProject(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Project updated successfully",
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
                    Message = $"Error updating project: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteProject(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Project deleted successfully"
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
                    Message = $"Error deleting project: {ex.Message}"
                });
            }
        }

        [HttpPost("{projectId}/send-email-to-pm")]
        [Authorize(Roles = "PROJECT_MANAGER, TEAM_LEADER")]
        public async Task<IActionResult> SendEmailToPM(int projectId)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.BadRequest,
                    Message = "Authorization token is required."
                });

            try
            {
                var result = await _service.SendEmailToProjectManager(projectId, token);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = result,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.InternalServerError,
                    Message = $"Error sending email: {ex.Message}"
                });
            }
        }

        [HttpPost("{projectId}/send-email-reject-to-leader")]
        [Authorize(Roles = "PROJECT_MANAGER, TEAM_LEADER")]
        public async Task<IActionResult> SendEmailToLeader(int projectId, [FromBody] ProjectRejectionRequestDTO request)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.BadRequest,
                    Message = "Authorization token is required."
                });

            try
            {
                var result = await _service.SendEmailToLeaderReject(projectId, token, request.Reason);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = result,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.InternalServerError,
                    Message = $"Error sending email: {ex.Message}"
                });
            }
        }





        [HttpPost("{projectId}/send-invitations")]
        [Authorize(Roles = "PROJECT_MANAGER, TEAM_LEADER")]
        public async Task<IActionResult> SendInvitationsToTeamMembers(int projectId)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];

            try
            {
                var result = await _service.SendInvitationsToTeamMembers(projectId, token);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = result,
                    Data = null
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponseDTO { IsSuccess = false, Code = 401, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error sending invitations: {ex.Message}"
                });
            }
        }


        [HttpGet("check-project-key")]
        public async Task<IActionResult> CheckProjectKey([FromQuery] string projectKey)
        {
            try
            {
                var exists = await _service.CheckProjectKeyExists(projectKey);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = $"Project key {projectKey} check completed",
                    Data = new { exists }
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
                    Message = $"Error checking project key: {ex.Message}"
                });
            }
        }

        [HttpGet("check-project-name")]
        public async Task<IActionResult> CheckProjectName([FromQuery] string projectName)
        {
            try
            {
                var exists = await _service.CheckProjectNameExists(projectName);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = $"Project name {projectName} check completed",
                    Data = new { exists }
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
                    Message = $"Error checking project key: {ex.Message}"
                });
            }
        }


        [HttpGet("view-by-key")]
        public async Task<IActionResult> GetProjectByKey([FromQuery] string projectKey)
        {
            try
            {
                var project = await _service.GetProjectByKey(projectKey);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = $"Project with key {projectKey} retrieved successfully",
                    Data = project
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
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
                    Message = $"Error retrieving project by key: {ex.Message}"
                });
            }
        }

        [HttpGet("by-project-key")]
        public async Task<IActionResult> GetProjectViewByKey([FromQuery] string projectKey)
        {
            try
            {
                var result = await _service.GetProjectViewByKeyAsync(projectKey);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = $"Project with key {projectKey} retrieved successfully",
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 404,
                    Message = "Project not found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error retrieving project by key: {ex.Message}"
                });
            }
        }


        [HttpPatch("{id}/status/{status}")]
        public async Task<IActionResult> ChangeStatus(int id,  string status)
        {
            try
            {
                var updated = await _service.ChangeProjectStatus(id, status);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Project status updated successfully",
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
                    Message = $"Error updating project status: {ex.Message}"
                });
            }
        }


       

    }
}
