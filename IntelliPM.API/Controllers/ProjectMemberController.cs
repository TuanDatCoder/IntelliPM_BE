using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Services.ProjectMemberServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/project/{projectId}/[controller]")] 
    [Authorize] 
    public class ProjectMemberController : ControllerBase
    {
        private readonly IProjectMemberService _service;

        public ProjectMemberController(IProjectMemberService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int projectId)
        {
            var result = await _service.GetAllProjectMembers(projectId);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all project members successfully",
                Data = result
            });
        }

  
        [HttpGet("accounts")] 
        public async Task<IActionResult> GetAccountsByProjectId(int projectId)
        {
            try
            {
                var result = await _service.GetAccountsByProjectId(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Accounts retrieved successfully for the project",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk(int projectId, [FromBody] List<ProjectMemberRequestDTO> requests)
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Request list cannot be null or empty." });
            }

            try
            {
                var results = new List<ProjectMemberResponseDTO>();
                foreach (var request in requests)
                {
                    if (!ModelState.IsValid || request.ProjectId != projectId)
                    {
                        return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data or project ID mismatch." });
                    }
                    var result = await _service.AddProjectMember(request);
                    results.Add(result);
                }
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Project members created successfully",
                    Data = results
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating project members: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(int projectId, [FromBody] ProjectMemberNoProjectIdRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateProjectMember(projectId,request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Project member created successfully",
                    Data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating project member: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int projectId, int id)
        {
            try
            {
                var member = await _service.GetProjectMemberById(id);
                if (member.ProjectId != projectId)
                    return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Project ID does not match." });

                await _service.DeleteProjectMember(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Project member deleted successfully"
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
                    Message = $"Error deleting project member: {ex.Message}"
                });
            }
        }

        [HttpGet("by-project")]
        public async Task<IActionResult> GetProjectMemberByProjectId(int projectId)
        {
            try
            {
                var files = await _service.GetProjectMemberByProjectId(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Retrieved member successfully.",
                    Data = files
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error retrieving member: {ex.Message}"
                });
            }
        }



        [HttpGet("by-account/{accountId}")]
        public async Task<IActionResult> GetProjectMemberByProjectIdAndAccountId(int projectId,int accountId)
        {
            try
            {
                var member = await _service.GetProjectMemberByProjectIdAndAccountId(projectId, accountId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Retrieved member successfully.",
                    Data = member
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error retrieving member: {ex.Message}"
                });
            }
        }



        [HttpPost("bulk-with-positions")]
        [Authorize(Roles = "PROJECT_MANAGER, TEAM_LEADER")]
        public async Task<IActionResult> CreateBulkWithPositions(int projectId, [FromBody] List<ProjectMemberWithPositionRequestDTO> requests)
        {

            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];

            if (requests == null || !requests.Any())
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.BadRequest,
                    Message = "Request list cannot be null or empty."
                });
            }

            try
            {
                var results = await _service.CreateBulkWithPositions(projectId, token, requests);
                return StatusCode((int)HttpStatusCode.Created, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.Created,
                    Message = "Project members and positions created successfully",
                    Data = results
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.InternalServerError,
                    Message = $"Error creating project members and positions: {ex.Message}"
                });
            }
        }

        [HttpGet("with-positions")]
        public async Task<IActionResult> GetProjectMemberWithPositions(int projectId)
        {
            try
            {
                var member = await _service.GetProjectMemberWithPositionsByProjectId(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Project member with positions retrieved successfully",
                    Data = member
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
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error retrieving project member with positions: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/status/{status}")]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            try
            {
                var updated = await _service.ChangeProjectMemberStatus(id, status);
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
                    Message = $"Error updating project member status: {ex.Message}"
                });
            }
        }

        [HttpGet("members/tasks")]
        public async Task<IActionResult> GetProjectMembersWithTasks(int projectId)
        {
            try
            {
                var result = await _service.GetProjectMembersWithTasksAsync(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Retrieved project member with tasks successfully",
                    Data = result
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
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error retrieving project member with tasks: {ex.Message}"
                });
            }

        }

        [HttpPatch("{id}/hourly-rate")]
        public async Task<IActionResult> ChangeHourlyRate(int id, [FromBody] decimal hourlyRate)
        {
            try
            {
                var updated = await _service.ChangeHourlyRate(id, hourlyRate);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Hourly rate updated successfully",
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
                    Message = $"Error updating hourly rate: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/working-hours-per-day")]
        public async Task<IActionResult> ChangeWorkingHoursPerDay(int id, [FromBody] decimal workingHoursPerDay)
        {
            try
            {
                var updated = await _service.ChangeWorkingHoursPerDay(id, workingHoursPerDay);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Working hours per day updated successfully",
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
                    Message = $"Error updating working hours per day: {ex.Message}"
                });
            }
        }
    }
}
