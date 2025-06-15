using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Services.ProjectMemberServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/[controller]")] 
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

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(int projectId, int id)
        //{
        //    try
        //    {
        //        var projectMember = await _service.GetProjectMemberById(id);
        //        if (projectMember.ProjectId != projectId)
        //            return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Project ID does not match." });

        //        return Ok(new ApiResponseDTO
        //        {
        //            IsSuccess = true,
        //            Code = (int)HttpStatusCode.OK,
        //            Message = "Project member retrieved successfully",
        //            Data = projectMember
        //        });
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> Create(int projectId, [FromBody] ProjectMemberRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            if (request.ProjectId != projectId)
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Project ID in request does not match URL." });

            try
            {
                var result = await _service.AddProjectMember(request);
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
    }
}