using IntelliPM.Services.ShareServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliPM.API.Controllers
{
    [Authorize] 
    [ApiController]
    [Route("api/share")] 
    public class DocumentShareController : ControllerBase
    {
        private readonly IShareTokenService _shareTokenService;

        public DocumentShareController(IShareTokenService shareTokenService)
        {
            _shareTokenService = shareTokenService;
        }

     
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User is not properly authenticated or accountId is missing.");
            }
            return userId;
        }

        [HttpPost("verify-token")]
        public IActionResult VerifyShareToken([FromBody] VerifyTokenRequest request)
        {
            if (string.IsNullOrEmpty(request?.Token))
            {
                return BadRequest(new { message = "Token is required." });
            }

            var tokenData = _shareTokenService.ValidateShareToken(request.Token);

            if (tokenData == null)
            {
                return BadRequest(new { message = "Link chia sẻ không hợp lệ hoặc đã hết hạn." });
            }

            var (documentId, accountIdInToken, permissionType) = tokenData.Value;

          
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId != accountIdInToken)
                {
                    return Forbid();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }


            return Ok(new
            {
                redirectUrl = $"/project/projects/form/document/{documentId}"
            });
        }
    }

  
    public class VerifyTokenRequest
    {
        public string Token { get; set; }
    }
}