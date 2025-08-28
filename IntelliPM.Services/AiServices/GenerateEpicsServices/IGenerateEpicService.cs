using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiServices.GenerateEpicsServices
{
    public interface IGenerateEpicService
    {
        Task<List<EpicPreviewDTO>> GenerateEpics(int projectId, List<string> existingEpicTitles);
    }
}
