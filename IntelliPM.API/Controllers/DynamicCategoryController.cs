using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.DynamicCategory.Request;
using IntelliPM.Services.DynamicCategoryServices;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/dynamic-categories")]
    public class DynamicCategoryController : ControllerBase
    {
        private readonly IDynamicCategoryService _dynamicCategoryService;

        public DynamicCategoryController(IDynamicCategoryService dynamicCategoryService)
        {
            _dynamicCategoryService = dynamicCategoryService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _dynamicCategoryService.GetAllDynamicCategories();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all dynamic categories successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _dynamicCategoryService.GetDynamicCategoryById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Dynamic category retrieved successfully",
                    Data = category
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpGet("by-name-or-category-group")]
        public async Task<IActionResult> GetByNameOrCategoryGroup([FromQuery] string name = null, [FromQuery] string categoryGroup = null)
        {
            try
            {
                var categories = await _dynamicCategoryService.GetDynamicCategoryByNameOrCategoryGroup(name, categoryGroup);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Dynamic categories retrieved successfully",
                    Data = categories
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DynamicCategoryRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _dynamicCategoryService.CreateDynamicCategory(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Dynamic category created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating dynamic category: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DynamicCategoryRequestDTO request)
        {
            try
            {
                var updated = await _dynamicCategoryService.UpdateDynamicCategory(id, request);
                return Ok(new ApiResponseDTO { IsSuccess = true, Code = 200, Message = "Updated successfully", Data = updated });
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
                await _dynamicCategoryService.DeleteDynamicCategory(id);
                return Ok(new ApiResponseDTO { IsSuccess = true, Code = 200, Message = "Deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var updated = await _dynamicCategoryService.ChangeDynamicCategoryStatus(id, isActive);
                return Ok(new ApiResponseDTO { IsSuccess = true, Code = 200, Message = "Status updated", Data = updated });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }
    }
}