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

        [HttpGet("by-risk-key")]
        public async Task<IActionResult> GetByKey(string key)
        {
            var result = await _riskService.GetByKeyAsync(key);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "Fetched risk detail successfully",
                Data = result
            });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status, int createdBy)
        {
            try
            {
                var updated = await _riskService.UpdateStatusAsync(id, status, createdBy);
                if (updated == null)
                    return NotFound($"Risk with ID {id} not found");

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Update risk status successfully",
                    data = updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update risk status: {ex.Message}");
            }
        }

        [HttpPatch("{id}/type")]
        public async Task<IActionResult> UpdateType(int id, [FromBody] string type)
        {
            try
            {
                var updated = await _riskService.UpdateTypeAsync(id, type);
                if (updated == null)
                    return NotFound($"Risk with ID {id} not found");

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Update risk type successfully",
                    data = updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update risk type: {ex.Message}");
            }
        }

        [HttpPatch("{id}/responsible-id")]
        public async Task<IActionResult> UpdateResponsibleId(int id, [FromBody] int? responsibleId)
        {
            try
            {
                var updated = await _riskService.UpdateResponsibleIdAsync(id, responsibleId);
                if (updated == null)
                    return NotFound($"Risk with ID {id} not found");

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Update risk responsible id successfully",
                    data = updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update risk responsible id: {ex.Message}");
            }
        }

        [HttpPatch("{id}/dueDate")]
        public async Task<IActionResult> UpdateDueDate(int id, [FromBody] DateTime dueDate)
        {
            try
            {
                var updated = await _riskService.UpdateDueDateAsync(id, dueDate);
                if (updated == null)
                    return NotFound($"Risk with ID {id} not found");

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Update risk due date successfully",
                    data = updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update risk due date: {ex.Message}");
            }
        }

        [HttpPatch("{id}/title")]
        public async Task<IActionResult> UpdateTitle(int id, [FromBody] string title, int createdBy)
        {
            try
            {
                var updated = await _riskService.UpdateTitleAsync(id, title, createdBy);
                if (updated == null)
                    return NotFound($"Risk with ID {id} not found");

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Update risk title successfully",
                    data = updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update risk title: {ex.Message}");
            }
        }

        [HttpPatch("{id}/description")]
        public async Task<IActionResult> UpdateDescription(int id, [FromBody] string description)
        {
            try
            {
                var updated = await _riskService.UpdateDescriptionAsync(id, description);
                if (updated == null)
                    return NotFound($"Risk with ID {id} not found");

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Update risk description successfully",
                    data = updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update risk description: {ex.Message}");
            }
        }

        [HttpPatch("{id}/impact-level")]
        public async Task<IActionResult> UpdateImpactLevel(int id, [FromBody] string impactLevel)
        {
            try
            {
                var updated = await _riskService.UpdateImpactLevelAsync(id, impactLevel);
                if (updated == null)
                    return NotFound($"Risk with ID {id} not found");

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Update risk impact level successfully",
                    data = updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update risk impact level: {ex.Message}");
            }
        }

        [HttpPatch("{id}/probability")]
        public async Task<IActionResult> UpdateProbability(int id, [FromBody] string probability)
        {
            try
            {
                var updated = await _riskService.UpdateProbabilityAsync(id, probability);
                if (updated == null)
                    return NotFound($"Risk with ID {id} not found");

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Update risk probability successfully",
                    data = updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update risk probability: {ex.Message}");
            }
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

        [HttpGet("ai-suggestion")]
        public async Task<IActionResult> ViewAIProjectRisksAsync([FromQuery] string projectKey)
        {
            try
            {
                var risks = await _riskService.ViewAIProjectRisksAsync(projectKey);

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

        [HttpGet("ai-suggestion-risk-task")]
        public async Task<IActionResult> ViewAIDetectTaskRisksAsyncAsync([FromQuery] string projectKey)
        {
            try
            {
                var risks = await _riskService.ViewAIDetectTaskRisksAsyncAsync(projectKey);
                if (!risks.Any())
                {
                    return Ok(new
                    {
                        isSuccess = true,
                        code = 200,
                        message = "No risks detected for the tasks. Check task data or Gemini API response.",
                        data = risks
                    });
                }

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Task risks detected successfully",
                    data = risks
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    code = 400,
                    message = $"Error detecting task risks: {ex.Message}"
                });
            }
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
