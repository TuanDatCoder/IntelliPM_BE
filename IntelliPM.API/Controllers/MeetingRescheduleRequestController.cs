using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.MeetingRescheduleRequest.Request;
using IntelliPM.Services.MeetingRescheduleRequestServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class MeetingRescheduleRequestController : ControllerBase
    {
        private readonly IMeetingRescheduleRequestService _service;

        public MeetingRescheduleRequestController(IMeetingRescheduleRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all reschedule requests successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Reschedule request retrieved successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MeetingRescheduleRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });

            var result = await _service.CreateAsync(request);
            return StatusCode(201, new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 201,
                Message = "Reschedule request created successfully",
                Data = result
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MeetingRescheduleRequestDTO request)
        {
            try
            {
                var result = await _service.UpdateAsync(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Reschedule request updated successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Reschedule request deleted successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpGet("by-requester/{requesterId}")]
        public async Task<IActionResult> GetByRequesterId(int requesterId)
        {
            var result = await _service.GetByRequesterIdAsync(requesterId);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "View reschedule requests by requester successfully",
                Data = result
            });
        }

    }
}