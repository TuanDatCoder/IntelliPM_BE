using IntelliPM.Data.DTOs;
using IntelliPM.Services.RecipientNotificationServices;
using IntelliPM.Services.SubtaskServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecipientNotificationController : ControllerBase
    {
        private readonly IRecipientNotificationService _service;

        public RecipientNotificationController(IRecipientNotificationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllRecipientNotification();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all Recipient Notification successfully",
                Data = result
            });
        }

        [HttpPut("mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromQuery] int accountId, [FromQuery] int notificationId)
        {
            await _service.MarkAsReadAsync(accountId, notificationId);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Notification marked as read successfully"
            });
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetByAccount(int accountId)
        {
            try
            {
                var recipientNotification = await _service.GetRecipientNotificationByAccount(accountId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = " RecipientNotification Notifications retrieved successfully",
                    Data = recipientNotification
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

    }
}
