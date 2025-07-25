﻿using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.ProjectMetricRepos
{
    public interface IProjectMetricRepository
    {
        Task<List<ProjectMetric>> GetAllAsync();
        Task<ProjectMetric?> GetByIdAsync(int id);
        //Task<List<ProjectMetric>> GetByProjectIdAsync(int projectId);
        Task<ProjectMetric?> GetByProjectIdAsync(int projectId);
        Task Add(ProjectMetric metric);
        Task Update(ProjectMetric metric);
        Task<ProjectMetric?> GetLatestByProjectIdAsync(int projectId);
        Task<ProjectMetric?> GetByProjectIdAndCalculatedByAsync(int projectId, string calculatedBy);

    }
}
