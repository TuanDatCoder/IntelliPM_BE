﻿ using IntelliPM.Data.DTOs;
using IntelliPM.Services.AccountServices;
using IntelliPM.Services.ProjectMemberServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IProjectMemberService _projectMemberService;

        public AccountController(IAccountService accountService, IProjectMemberService projectMemberService)
        {
            _accountService = accountService;
            _projectMemberService = projectMemberService;
        }

        [HttpPost("{accountId}/upload-avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(int accountId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.BadRequest,
                    Message = "File is not selected"
                });
            }

            try
            {
                using var fileStream = file.OpenReadStream();
                string fileUrl = await _accountService.UploadProfilePictureAsync(accountId, fileStream, file.FileName);

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Avatar uploaded successfully",
                    Data = new { FileUrl = fileUrl }
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("upload-avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatarByToken(IFormFile file)
        {
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.BadRequest,
                    Message = "File is not selected"
                });
            }

            try
            {
                using var fileStream = file.OpenReadStream();
                string fileUrl = await _accountService.UploadPictureAsync(token, fileStream, file.FileName);

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Avatar uploaded successfully",
                    Data = new { FileUrl = fileUrl }
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message
                });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeAccountStatus(int id, [FromBody] string newStatus)
        {
            try
            {
                var updatedOrder = await _accountService.ChangeAccountStatus(id, newStatus);

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Account status updated successfully",
                    Data = updatedOrder
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.NotFound,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
        }


        [HttpGet("{accountId}/projects")]
        public async Task<IActionResult> GetProjectsByAccountId(int accountId)
        {
            try
            {
                
            
                var result = await _projectMemberService.GetProjectsByAccountId(accountId);

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Projects retrieved successfully for the account",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
        }
        }

        [HttpGet("projects")]
        [Authorize]
        public async Task<IActionResult> GetProjectsByAccount()
        {
            try
            {

                var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new ApiResponseDTO { IsSuccess = false, Code = 401, Message = "Unauthorized" });
                }
                var result = await _projectMemberService.GetProjectsByAccount(token);

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Projects retrieved successfully for the account",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetById(string email)
        {
            try
            {
                var category = await _accountService.GetAccountByEmail(email);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Account retrieved successfully",
                    Data = category
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }


    }
}
