using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.ProjectPosition.Request;
using IntelliPM.Services.ProjectPositionServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/project-member/{projectMemberId}/[controller]")]
    [Authorize] // Yêu cầu xác thực cho toàn bộ controller
    public class ProjectPositionController : ControllerBase
    {
        private readonly IProjectPositionService _service;

        public ProjectPositionController(IProjectPositionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int projectMemberId)
        {
            var result = await _service.GetAllProjectPositions(projectMemberId);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all project positions successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int projectMemberId, int id)
        {
            try
            {
                var position = await _service.GetProjectPositionById(id);
                if (position.ProjectMemberId != projectMemberId)
                    return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Project member ID does not match." });

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Project position retrieved successfully",
                    Data = position
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(int projectMemberId, [FromBody] ProjectPositionRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            if (request.ProjectMemberId != projectMemberId)
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Project member ID in request does not match URL." });

            try
            {
                var result = await _service.AddProjectPosition(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Project position added successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error adding project position: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int projectMemberId, int id, [FromBody] ProjectPositionRequestDTO request)
        {
            try
            {
                var position = await _service.GetProjectPositionById(id);
                if (position.ProjectMemberId != projectMemberId)
                    return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Project member ID does not match." });

                var updated = await _service.UpdateProjectPosition(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Project position updated successfully",
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
                    Message = $"Error updating project position: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int projectMemberId, int id)
        {
            try
            {
                var position = await _service.GetProjectPositionById(id);
                if (position.ProjectMemberId != projectMemberId)
                    return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Project member ID does not match." });

                await _service.DeleteProjectPosition(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Project position deleted successfully"
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
                    Message = $"Error deleting project position: {ex.Message}"
                });
            }
        }
    }
}
