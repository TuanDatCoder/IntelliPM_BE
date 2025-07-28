using AutoMapper;
using Google.Cloud.Storage.V1;
using IntelliPM.Data.DTOs.Risk.Request;
using IntelliPM.Data.DTOs.Risk.Response;
using IntelliPM.Data.DTOs.RiskSolution.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.RiskRepos;
using IntelliPM.Repositories.RiskSolutionRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.GeminiServices;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RiskServices
{
    public class RiskService : IRiskService
    {
        private readonly IRiskRepository _riskRepo;
        private readonly IRiskSolutionRepository _riskSolutionRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IGeminiService _geminiService;
        private readonly IMapper _mapper;

        public RiskService(IRiskRepository riskRepo, IRiskSolutionRepository riskSolutionRepo, IGeminiService geminiService, ITaskRepository taskRepo, IProjectRepository projectRepo, IProjectMemberRepository projectMemberRepo, IMapper mapper)
        {
            _riskRepo = riskRepo;
            _riskSolutionRepo = riskSolutionRepo;
            _taskRepo = taskRepo;
            _projectRepo = projectRepo;
            _projectMemberRepo = projectMemberRepo;
            _geminiService = geminiService;
            _mapper = mapper;
        }

        public async Task<List<RiskResponseDTO>> GetAllRisksAsync()
        {
            var risks = await _riskRepo.GetAllRisksAsync();
            return _mapper.Map<List<RiskResponseDTO>>(risks);
        }

        public async Task<List<RiskResponseDTO>> GetByProjectIdAsync(int projectId)
        {
            var risks = await _riskRepo.GetByProjectIdAsync(projectId);
            return _mapper.Map<List<RiskResponseDTO>>(risks);
        }

        public async Task<RiskResponseDTO> GetByIdAsync(int id)
        {
            var risk = await _riskRepo.GetByIdAsync(id)
                ?? throw new Exception("Risk not found");
            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task DeleteAsync(int id)
        {
            var risk = await _riskRepo.GetByIdAsync(id)
                ?? throw new Exception("Risk not found");
            await _riskRepo.DeleteAsync(risk);
        }

        public async Task<List<RiskResponseDTO>> GetUnapprovedAIRisksAsync(int projectId)
        {
            var risks = await _riskRepo.GetUnapprovedAIRisksByProjectIdAsync(projectId);
            return _mapper.Map<List<RiskResponseDTO>>(risks);
        }

        public async Task ApproveRiskAsync(RiskRequestDTO dto, RiskSolutionRequestDTO solutionDto)
        {
            var risk = _mapper.Map<Risk>(dto);
            risk.IsApproved = true;
            risk.CreatedAt = DateTime.UtcNow;
            risk.UpdatedAt = DateTime.UtcNow;

            await _riskRepo.AddAsync(risk);

            var solution = _mapper.Map<RiskSolution>(solutionDto);
            solution.RiskId = risk.Id;
            solution.CreatedAt = DateTime.UtcNow;
            solution.UpdatedAt = DateTime.UtcNow;

            await _riskSolutionRepo.AddAsync(solution);
        }

        public async Task<List<RiskRequestDTO>> DetectAndSaveProjectRisksAsync(int projectId)
        {
            var project = await _projectRepo.GetByIdAsync(projectId)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(projectId);
            var risks = await _geminiService.DetectProjectRisksAsync(project, tasks);
            if (risks == null || !risks.Any())
                throw new Exception("AI did not return valid project risks");

            var savedRisks = new List<RiskRequestDTO>();

            foreach (var riskDto in risks)
            {
                var riskEntity = _mapper.Map<Risk>(riskDto);
                riskEntity.ProjectId = project.Id;
                riskEntity.TaskId = null;
                riskEntity.ResponsibleId = 1; // đổi lại có thể cho null được
                riskEntity.RiskScope = "Project";
                riskEntity.GeneratedBy = "AI";
                riskEntity.Status = "Pending";
                riskEntity.IsApproved = false;
                riskEntity.CreatedAt = DateTime.UtcNow;
                riskEntity.UpdatedAt = DateTime.UtcNow;

                await _riskRepo.AddAsync(riskEntity);

            var solutionEntity = new RiskSolution
                {
                    RiskId = riskEntity.Id,
                    MitigationPlan = riskDto.MitigationPlan,
                    ContingencyPlan = riskDto.ContingencyPlan,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _riskSolutionRepo.AddAsync(solutionEntity);

                var savedDto = _mapper.Map<RiskRequestDTO>(riskEntity);
                savedDto.MitigationPlan = riskDto.MitigationPlan;
                savedDto.ContingencyPlan = riskDto.ContingencyPlan;

                savedRisks.Add(savedDto);
            }

            return savedRisks;
        }

        public async Task<List<RiskRequestDTO>> DetectProjectRisksAsync(int projectId)
        {
            var project = await _projectRepo.GetByIdAsync(projectId)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(projectId);
            var risks = await _geminiService.DetectProjectRisksAsync(project, tasks);

            return risks ?? new List<RiskRequestDTO>();
        }

        public async Task<List<RiskRequestDTO>> SaveProjectRisksAsync(List<RiskRequestDTO> risks)
        {
            var saved = new List<RiskRequestDTO>();

            foreach (var dto in risks)
            {
                var riskEntity = _mapper.Map<Risk>(dto);
                riskEntity.Id = 0; // reset nếu dùng lại dto
                riskEntity.ProjectId = dto.ProjectId;
                riskEntity.Status = "Approved";
                riskEntity.GeneratedBy = "AI";
                riskEntity.IsApproved = true;
                riskEntity.CreatedAt = DateTime.UtcNow;
                riskEntity.UpdatedAt = DateTime.UtcNow;

                try
                {
                    await _riskRepo.AddAsync(riskEntity);

                    var solution = new RiskSolution
                    {
                        RiskId = riskEntity.Id, // <-- nếu Id vẫn = 0 => lỗi
                        MitigationPlan = dto.MitigationPlan,
                        ContingencyPlan = dto.ContingencyPlan,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _riskSolutionRepo.AddAsync(solution);
                    var savedDto = _mapper.Map<RiskRequestDTO>(riskEntity);
                    savedDto.MitigationPlan = solution.MitigationPlan;
                    savedDto.ContingencyPlan = solution.ContingencyPlan;

                    saved.Add(savedDto);
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi khi lưu Risk hoặc RiskSolution:\n" + ex.InnerException?.Message ?? ex.Message);
                }


                //await _riskRepo.AddAsync(riskEntity);
                //if (riskEntity.Id == 0)
                //    throw new Exception("Risk entity ID not set after save.");

                //var solution = new RiskSolution
                //{
                //    RiskId = riskEntity.Id,
                //    MitigationPlan = dto.MitigationPlan,
                //    ContingencyPlan = dto.ContingencyPlan,
                //    CreatedAt = DateTime.UtcNow,
                //    UpdatedAt = DateTime.UtcNow
                //};

                //await _riskSolutionRepo.AddAsync(solution);

                //var savedDto = _mapper.Map<RiskRequestDTO>(riskEntity);
                //savedDto.MitigationPlan = solution.MitigationPlan;
                //savedDto.ContingencyPlan = solution.ContingencyPlan;

                //saved.Add(savedDto);
            }

            return saved;
        }

        public async Task<List<RiskResponseDTO>> GetByProjectKeyAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var risks = await _riskRepo.GetByProjectIdAsync(project.Id);

            return _mapper.Map<List<RiskResponseDTO>>(risks);
        }

        public async Task<RiskResponseDTO> CreateRiskAsync(RiskCreateRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Title is required.");
            if (string.IsNullOrWhiteSpace(request.RiskScope))
                throw new ArgumentException("RiskScope is required.");
            if (string.IsNullOrWhiteSpace(request.ProjectKey))
                throw new ArgumentException("ProjectKey is required.");

            var project = await _projectRepo.GetProjectByKeyAsync(request.ProjectKey)
                ?? throw new Exception("Project not found with provided projectKey");

            var count = await _riskRepo.CountByProjectIdAsync(project.Id);
            var nextIndex = count + 1;
            var riskKey = $"{project.ProjectKey}-R{nextIndex:D3}";

            var impactLevel = request.ImpactLevel;
            var probability = request.Probability;

            var entity = _mapper.Map<Risk>(request);
            entity.ProjectId = project.Id;
            entity.RiskKey = riskKey;
            entity.SeverityLevel = CalculateSeverityLevel(impactLevel, probability);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await _riskRepo.AddAsync(entity); 
            return _mapper.Map<RiskResponseDTO>(entity);
        }

        public async Task<RiskResponseDTO?> UpdateStatusAsync(int id, string status)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            risk.Status = status;
            risk.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _riskRepo.UpdateAsync(risk);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update risk status: {ex.Message}", ex);
            }
            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<RiskResponseDTO?> UpdateTypeAsync(int id, string type)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            risk.Type = type;
            risk.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _riskRepo.UpdateAsync(risk);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update risk type: {ex.Message}", ex);
            }
            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<RiskResponseDTO?> UpdateResponsibleIdAsync(int id, int? responsibleId)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            risk.ResponsibleId = responsibleId;
            risk.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _riskRepo.UpdateAsync(risk);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update risk responsible id: {ex.Message}", ex);
            }
            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<RiskResponseDTO?> UpdateDueDateAsync(int id, DateTime dueDate)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            risk.DueDate = dueDate;
            risk.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _riskRepo.UpdateAsync(risk);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change risk due date: {ex.Message}", ex);
            }

            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<RiskResponseDTO?> UpdateTitleAsync(int id, string title)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            risk.Title = title;
            risk.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _riskRepo.UpdateAsync(risk);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change risk title: {ex.Message}", ex);
            }

            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<RiskResponseDTO?> UpdateDescriptionAsync(int id, string description)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            risk.Description = description;
            risk.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _riskRepo.UpdateAsync(risk);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change risk description: {ex.Message}", ex);
            }

            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<RiskResponseDTO?> UpdateImpactLevelAsync(int id, string impactLevel)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            risk.ImpactLevel = impactLevel;
            risk.UpdatedAt = DateTime.UtcNow;

            var probability = risk.Probability; 
            risk.SeverityLevel = CalculateSeverityLevel(impactLevel, probability);

            try
            {
                await _riskRepo.UpdateAsync(risk);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change risk impact level: {ex.Message}", ex);
            }

            return _mapper.Map<RiskResponseDTO>(risk);
        }

        private string CalculateSeverityLevel(string impact, string probability)
        {
            int GetLevelValue(string level)
            {
                return level.ToLower() switch
                {
                    "low" => 1,
                    "medium" => 2,
                    "high" => 3,
                    _ => 0
                };
            }

            var impactValue = GetLevelValue(impact);
            var probValue = GetLevelValue(probability);

            return (impactValue, probValue) switch
            {
                (1, 1) => "Low",
                (1, 2) => "Low",
                (1, 3) => "Medium",
                (2, 1) => "Low",
                (2, 2) => "Medium",
                (2, 3) => "High",
                (3, 1) => "Medium",
                (3, 2) => "High",
                (3, 3) => "High",
                _ => "Unknown"
            };
        }

        public async Task<RiskResponseDTO?> UpdateProbabilityAsync(int id, string probability)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            risk.Probability = probability;
            risk.UpdatedAt = DateTime.UtcNow;

            var impactLevel = risk.ImpactLevel;
            risk.SeverityLevel = CalculateSeverityLevel(impactLevel, probability);

            try
            {
                await _riskRepo.UpdateAsync(risk);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change risk probability: {ex.Message}", ex);
            }

            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<List<AIRiskResponseDTO>> ViewAIProjectRisksAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var risks = await _geminiService.ViewAIProjectRisksAsync(project, tasks);

            return risks ?? new List<AIRiskResponseDTO>();
        }
    }

}
