using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.Epic.Request;
using IntelliPM.Services.EpicServices;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EpicController : ControllerBase
    {
        private readonly IEpicService _service;

        public EpicController(IEpicService service)
        {
            _service = service;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllEpics();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all epics successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var epic = await _service.GetEpicById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Epic retrieved successfully",
                    Data = epic
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            try
            {
                var epics = await _service.GetEpicByName(name);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Epics retrieved successfully",
                    Data = epics
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EpicRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateEpic(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Epic created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating epic: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EpicRequestDTO request)
        {
            try
            {
                var updated = await _service.UpdateEpic(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Epic updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating epic: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteEpic(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Epic deleted successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error deleting epic: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] string status)
        {
            try
            {
                var updated = await _service.ChangeEpicStatus(id, status);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Epic status updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating epic status: {ex.Message}"
                });
            }
        }
    }
}
