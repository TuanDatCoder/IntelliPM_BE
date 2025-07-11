﻿using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.ProjectPosition.Request;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Services.ProjectPositionServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/project-member/{projectMemberId}/[controller]")]
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
        public async Task<IActionResult> Add(int projectMemberId, [FromBody] ProjectPositionNoMemberIdRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateProjectPosition( projectMemberId,request);
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
        [HttpPost("bulk")]
        public async Task<IActionResult> AddBulk(int projectMemberId, [FromBody] List<ProjectPositionRequestDTO> requests)
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Request list cannot be null or empty." });
            }

            try
            {
                var results = new List<ProjectPositionResponseDTO>();
                foreach (var request in requests)
                {
                    if (!ModelState.IsValid || request.ProjectMemberId != projectMemberId)
                    {
                        return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data or project member ID mismatch." });
                    }
                    var result = await _service.AddProjectPosition(request);
                    results.Add(result);
                }
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Project positions added successfully",
                    Data = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error adding project positions: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int projectMemberId, int id, ProjectPositionNoMemberIdRequestDTO request)
        {
            try
            {
                

                var updated = await _service.UpdateProjectPosition(id, projectMemberId, request);
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
