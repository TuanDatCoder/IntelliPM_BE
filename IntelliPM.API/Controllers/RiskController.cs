using IntelliPM.Data.DTOs.Risk.Request;
using IntelliPM.Data.DTOs;
using IntelliPM.Services.RiskServices;
using Microsoft.AspNetCore.Mvc;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.TaskRepos;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiskController : ControllerBase
    {
        private readonly IRiskService _riskService;
        private readonly IProjectRepository _projectRepo;
        private readonly ITaskRepository _taskRepo;

        public RiskController(IRiskService riskService, IProjectRepository projectRepo, ITaskRepository taskRepo)
        {
            _riskService = riskService;
            _projectRepo = projectRepo;
            _taskRepo = taskRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _riskService.GetAllRisksAsync();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "Fetched all risks successfully",
                Data = result
            });
        }

        [HttpGet("by-project-id")]
        public async Task<IActionResult> GetByProjectId([FromQuery] int projectId)
        {
            try
            {
                var result = await _riskService.GetByProjectIdAsync(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Fetched risks by project",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Internal Server Error: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpGet("by-project-key")]
        public async Task<IActionResult> GetByProjectKey([FromQuery] string projectKey)
        {
            try
            {
                var result = await _riskService.GetByProjectKeyAsync(projectKey);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Fetched risks by project",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Internal Server Error: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _riskService.GetByIdAsync(id);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "Fetched risk detail successfully",
                Data = result
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RiskRequestDTO request)
        {
            await _riskService.UpdateAsync(id, request);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "Updated risk successfully"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _riskService.DeleteAsync(id);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "Deleted risk successfully"
            });
        }

        [HttpGet("unapproved-ai-risks")]
        public async Task<IActionResult> GetUnapprovedAIRisks([FromQuery] int projectId)
        {
            var risks = await _riskService.GetUnapprovedAIRisksAsync(projectId);
            return Ok(new { isSuccess = true, data = risks });
        }

        //[HttpPost("approve-ai-risk")]
        //public async Task<IActionResult> ApproveRisk([FromBody] RiskApprovalDTO dto)
        //{
        //    await _riskService.ApproveRiskAsync(dto.Risk, dto.Solution);
        //    return Ok(new { isSuccess = true, message = "Approved and saved successfully." });
        //}

        [HttpPost("detect-and-save-project-risks")]
        public async Task<IActionResult> DetectAndSaveProjectRisks([FromQuery] int projectId)
        {
            try
            {
                var result = await _riskService.DetectAndSaveProjectRisksAsync(projectId);
                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Project risks detected and saved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO { IsSuccess = false, Code = 500, Message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpGet("view-project-risks")]
        public async Task<IActionResult> ViewProjectRisks([FromQuery] int projectId)
        {
            try
            {
                var risks = await _riskService.DetectProjectRisksAsync(projectId);

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Project risks detected successfully",
                    data = risks
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    code = 400,
                    message = ex.Message
                });
            }
        }

        [HttpPost("save-approved-risks")]
        public async Task<IActionResult> SaveApprovedRisks([FromBody] List<RiskRequestDTO> risks)
        {
            try
            {
                if (risks == null || !risks.Any())
                    return BadRequest(new { isSuccess = false, message = "Empty risk list" });

                var saved = await _riskService.SaveProjectRisksAsync(risks);

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Risks saved successfully",
                    data = saved
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    code = 500,
                    message = ex.Message
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RiskCreateRequestDTO request)
        {
            try
            {
                var result = await _riskService.CreateRiskAsync(request);
                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    data = result,
                    message = "Risk created successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    code = 400,
                    message = ex.Message
                });
            }
        }


    }
}
