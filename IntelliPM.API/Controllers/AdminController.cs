
using Google.Api;
using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.Admin;
using IntelliPM.Data.DTOs.Admin.Request;
using IntelliPM.Services.AdminServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("account")]
        [Authorize(Roles = "ADMIN,PROJECT_MANAGER")]
        //[Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAccounts()
        {
            var result = await _adminService.GetAllAccountsAsync();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Accounts retrieved successfully",
                Data = result
            });
        }

        [HttpGet("project-status")]
        public async Task<IActionResult> GetProjectStatusReports()
        {
            var result = await _adminService.GetProjectStatusReportsAsync();
            return Ok(result);
        }

        [HttpGet("project-managers")]
        public async Task<ActionResult<List<ProjectManagerReportDto>>> GetProjectManagerReports()
        {
            var reports = await _adminService.GetProjectManagerReportsAsync();
            return Ok(reports);
        }

        [HttpPost("register-account")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> RegisterAccount([FromBody] List<AdminAccountRequestDTO> requests)
        {
            if (requests == null || requests.Count == 0)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.BadRequest,
                    Message = "Request list cannot be empty",
                    Data = null
                });
            }

            try
            {
                var result = await _adminService.RegisterAccountAsync(requests);
                var message = result.Failed.Count == 0
                    ? "All accounts registered successfully"
                    : $"Registered {result.Successful.Count} account(s) successfully, {result.Failed.Count} failed";

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = result.Failed.Count == 0,
                    Code = (int)HttpStatusCode.OK,
                    Message = message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred: " + ex.Message,
                    Data = null
                });
            }
        }

    }
}
