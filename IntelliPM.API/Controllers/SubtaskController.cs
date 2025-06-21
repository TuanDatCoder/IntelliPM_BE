using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs;
using IntelliPM.Services.TaskServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using IntelliPM.Services.SubtaskServices;
using IntelliPM.Data.DTOs.TaskCheckList.Request;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class SubtaskController : ControllerBase
    {
        private readonly ISubtaskService _service;

        public SubtaskController(ISubtaskService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllSubtaskList();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all task check list successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var taskCheckList = await _service.GetSubtaskById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Task check list retrieved successfully",
                    Data = taskCheckList
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubtaskRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateSubtask(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Task check list created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating task check list: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] SubtaskRequestDTO request)
        {
            try
            {
                var updated = await _service.UpdateSubtask(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task check list updated successfully",
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
                    Message = $"Error updating task check list: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _service.DeleteSubtask(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task check list deleted successfully"
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
                    Message = $"Error deleting task check list: {ex.Message}"
                });
            }
        }

        //[HttpPatch("{id}/status")]
        //public async Task<IActionResult> ChangeStatus(int id, [FromBody] string status)
        //{
        //    try
        //    {
        //        var updated = await _service.ChangeTaskStatus(id, status);
        //        return Ok(new ApiResponseDTO
        //        {
        //            IsSuccess = true,
        //            Code = 200,
        //            Message = "Task status updated successfully",
        //            Data = updated
        //        });
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new ApiResponseDTO
        //        {
        //            IsSuccess = false,
        //            Code = 500,
        //            Message = $"Error updating task status: {ex.Message}"
        //        });
        //    }
        //}
    }
}
