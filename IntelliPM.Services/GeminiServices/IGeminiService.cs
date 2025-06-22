using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.GeminiServices
{
    public interface IGeminiService
    {
        Task<List<string>> GenerateSubtaskAsync(string taskTitle);
    }

}
