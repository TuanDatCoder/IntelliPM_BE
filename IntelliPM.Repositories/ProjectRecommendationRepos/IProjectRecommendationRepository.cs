using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.ProjectRecommendationRepos
{
    public interface IProjectRecommendationRepository
    {
        Task<List<ProjectRecommendation>> GetByProjectIdAsync(int projectId);
        Task Add(ProjectRecommendation recommendation);
    }

}
