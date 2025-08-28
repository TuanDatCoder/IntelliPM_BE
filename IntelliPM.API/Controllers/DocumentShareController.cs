using IntelliPM.Services.ShareServices;
 // <-- Bạn cần tạo service này theo hướng dẫn trước
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliPM.API.Controllers
{
    [Authorize] // Bắt buộc người dùng phải đăng nhập để truy cập các endpoint trong này
    [ApiController]
    [Route("api/share")] // Route riêng cho việc xác thực chia sẻ
    public class DocumentShareController : ControllerBase
    {
        private readonly IShareTokenService _shareTokenService;

        public DocumentShareController(IShareTokenService shareTokenService)
        {
            _shareTokenService = shareTokenService;
        }

        /// <summary>
        /// Lấy ID của người dùng đang đăng nhập từ token.
        /// Đảm bảo claim "accountId" tồn tại trong JWT của bạn.
        /// </summary>
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

            // KIỂM TRA BẢO MẬT QUAN TRỌNG NHẤT:
            // Người dùng đang đăng nhập (từ token xác thực chính) có phải là người được mời không (từ token chia sẻ)?
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId != accountIdInToken)
                {
                    // Trả về lỗi 403 Forbidden nếu người dùng không đúng
                    return Forbid();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }


            // Nếu mọi thứ hợp lệ, trả về URL để frontend chuyển hướng
            return Ok(new
            {
                redirectUrl = $"/project/projects/form/document/{documentId}"
            });
        }
    }

    /// <summary>
    /// DTO cho request body của API verify-token
    /// </summary>
    public class VerifyTokenRequest
    {
        public string Token { get; set; }
    }
}