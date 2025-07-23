using IntelliPM.Data.DTOs;
using IntelliPM.Services.NotificationServices;
using IntelliPM.Services.SubtaskServices;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetByAccount(int accountId)
        {
            try
            {
                var notifications = await _service.GetNotificationByAccount(accountId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Notifications retrieved successfully",
                    Data = notifications
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllNotificationList();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all notification successfully",
                Data = result
            });
        }
    }
}
