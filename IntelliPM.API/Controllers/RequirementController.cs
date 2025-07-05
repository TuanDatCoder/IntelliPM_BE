using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.Requirement.Request;
using IntelliPM.Services.RequirementServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/project/{projectId}/[controller]")]

    public class RequirementController : ControllerBase
    {
        private readonly IRequirementService _service;

        public RequirementController(IRequirementService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int projectId)
        {
            var result = await _service.GetAllRequirements(projectId);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all requirements successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int projectId, int id)
        {
            try
            {
                var requirement = await _service.GetRequirementById(id);
                if (requirement.ProjectId != projectId)
                    return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Project ID does not match." });

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Requirement retrieved successfully",
                    Data = requirement
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpGet("by-title")]
        public async Task<IActionResult> GetByTitle(int projectId, [FromQuery] string title)
        {
            try
            {
                var requirements = await _service.GetRequirementByTitle(title);
                if (!requirements.Any(r => r.ProjectId == projectId))
                    return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = "No requirements found for this project with the given title." });

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Requirements retrieved successfully",
                    Data = requirements
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
        public async Task<IActionResult> Create(int projectId, [FromBody] RequirementRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            if (request.ProjectId != projectId)
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Project ID in request does not match URL." });

            try
            {
                var result = await _service.CreateRequirement(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Requirement created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating requirement: {ex.Message}"
                });
            }
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "PROJECT_MANAGER, TEAM_LEADER")]
        public async Task<IActionResult> CreateBulk(int projectId, [FromBody] List<RequirementBulkRequestDTO> requests)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            if (requests == null || !requests.Any())
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "List of requirements cannot be null or empty." });
            }

            try
            {
        
                var result = await _service.CreateListRequirement(projectId, requests);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Requirements created successfully",
                    Data = result
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
                    Message = $"Error creating requirements: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int projectId, int id, [FromBody] RequirementRequestDTO request)
        {
            try
            {
                var requirement = await _service.GetRequirementById(id);
                if (requirement.ProjectId != projectId)
                    return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Project ID does not match." });

                request.ProjectId = projectId; // Đảm bảo projectId không bị thay đổi
                var updated = await _service.UpdateRequirement(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Requirement updated successfully",
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
                    Message = $"Error updating requirement: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int projectId, int id)
        {
            try
            {
                var requirement = await _service.GetRequirementById(id);
                if (requirement.ProjectId != projectId)
                    return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Project ID does not match." });

                await _service.DeleteRequirement(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Requirement deleted successfully"
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
                    Message = $"Error deleting requirement: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/priority")]
        public async Task<IActionResult> ChangePriority(int projectId, int id, [FromBody] string priority)
        {
            try
            {
                var requirement = await _service.GetRequirementById(id);
                if (requirement.ProjectId != projectId)
                    return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Project ID does not match." });

                var updated = await _service.ChangeRequirementPriority(id, priority);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Requirement priority updated successfully",
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
                    Message = $"Error updating requirement priority: {ex.Message}"
                });
            }
        }
    }
}
