using IntelliPM.Data.DTOs;
using IntelliPM.Services.RecipientNotificationServices;
using IntelliPM.Services.SubtaskServices;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
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
    }
}
