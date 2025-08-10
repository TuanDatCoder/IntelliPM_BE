using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MetricHistoryRepos
{
    public interface IMetricHistoryRepository
    {
        Task Add(ProjectMetricHistory metricHistory);
        Task<List<ProjectMetricHistory>> GetByProjectIdAsync(int projectId);
    }
}
