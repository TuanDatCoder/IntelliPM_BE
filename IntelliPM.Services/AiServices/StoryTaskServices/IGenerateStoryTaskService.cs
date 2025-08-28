using IntelliPM.Data.DTOs.Ai.GenerateStoryTask.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiServices.StoryTaskServices
{
    public interface IGenerateStoryTaskService
    {
        Task<List<object>> GenerateStoryOrTask(int id,GenerateStoryTaskRequestDTO request);
    }
}
