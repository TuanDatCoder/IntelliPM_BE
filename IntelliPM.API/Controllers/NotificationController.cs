using IntelliPM.Data.DTOs;
using IntelliPM.Services.NotificationServices;
using IntelliPM.Services.SubtaskServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace IntelliPM.API.Controllers
{
[ApiController]
[Route("api/[controller]")]
    //[Authorize]
    public class NotificationController : ControllerBase
{
        private readonly INotificationService _service;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetByAccount(int accountId)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationByAccount(accountId);
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
            var result = await _notificationService.GetAllNotificationList();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all notification successfully",
                Data = result
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var accountIdClaim = User.FindFirst("accountId")?.Value;

            if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out var userId))
                return Unauthorized();

            var notifications = await _notificationService.GetNotificationsByUserId(userId);
            return Ok(notifications);
        }

    }
}
