using AutoMapper;
using IntelliPM.Data.DTOs.ProjectMetricHistory.Response;
using IntelliPM.Repositories.MetricHistoryRepos;
using IntelliPM.Repositories.ProjectRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectMetricHistoryServices
{
    public class ProjectMetricHistoryService : IProjectMetricHistoryService
    {
        private readonly IMetricHistoryRepository _repo;
        private readonly IProjectRepository _projectRepo;
        private readonly IMapper _mapper;

        public ProjectMetricHistoryService(IMetricHistoryRepository repo, IProjectRepository projectRepo, IMapper mapper)
        {
            _repo = repo;
            _projectRepo = projectRepo;
            _mapper = mapper;
        }

        public async Task<List<ProjectMetricHistoryResponseDTO>> GetByProjectKeyAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var history = await _repo.GetByProjectIdAsync(project.Id);
            return _mapper.Map<List<ProjectMetricHistoryResponseDTO>>(history);
        }
    }
}
