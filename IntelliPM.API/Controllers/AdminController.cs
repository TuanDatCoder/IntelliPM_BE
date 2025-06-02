
using IntelliPM.Data.DTOs;
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
        //[Authorize(Roles = "ADMIN,PROJECT MANAGER")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAccounts([FromQuery] int? page, [FromQuery] int? size)
        {
            var result = await _adminService.GetAllAccountsAsync(page, size);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Accounts retrieved successfully",
                Data = result
            });
        }
      
    }
}
