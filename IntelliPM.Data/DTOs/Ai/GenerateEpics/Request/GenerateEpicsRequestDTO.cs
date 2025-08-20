using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Ai.GenerateEpics.Request
{
    public class GenerateEpicsRequestDTO
    {
        public List<string> ExistingEpicTitles { get; set; } = new List<string>();
    }
}
