using IntelliPM.Data.DTOs.ProjectRecommendation.Request;
using IntelliPM.Data.DTOs.ProjectRecommendation.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectRecommendationServices
{
    public interface IProjectRecommendationService
    {
        Task<List<AIRecommendationDTO>> GenerateProjectRecommendationsAsync(string projectKey);
        Task CreateAsync(ProjectRecommendationRequestDTO dto);
    }
}
