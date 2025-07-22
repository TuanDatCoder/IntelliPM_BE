using IntelliPM.Services.NotificationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
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
