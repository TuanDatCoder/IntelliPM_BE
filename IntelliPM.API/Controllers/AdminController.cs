//using Data.DTOs;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Services.AdminServices;
//using Services.BrandServices;
//using System.Net;

//namespace ConstructionEquipmentRental.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AdminController : ControllerBase
//    {
//        private readonly IAdminService _adminService;

//        public AdminController(IAdminService adminService)
//        {
//            _adminService = adminService;
//        }

//        [HttpGet("account")]
//       // [Authorize(Roles = "ADMIN,CUSTOMER")]
//       [Authorize(Roles = "ADMIN")]
//        public async Task<IActionResult> GetAccounts([FromQuery] int? page, [FromQuery] int? size)
//        {
//            var result = await _adminService.GetAllAccountsAsync(page, size);
//            return Ok(new ApiResponseDTO
//            {
//                IsSuccess = true,
//                Code = (int)HttpStatusCode.OK,
//                Message = "Accounts retrieved successfully",
//                Data = result
//            });
//        }
//        [HttpGet("dashboard")]
//        [Authorize(Roles = "ADMIN")]
//        public async Task<IActionResult> GetDashboardStats()
//        {
//            var stats = await _adminService.GetDashboardStatsAsync();
//            return Ok(new ApiResponseDTO
//            {
//                IsSuccess = true,
//                Code = (int)HttpStatusCode.OK,
//                Message = "Dashboard stats retrieved successfully",
//                Data = stats
//            });
//        }
//    }
//}
