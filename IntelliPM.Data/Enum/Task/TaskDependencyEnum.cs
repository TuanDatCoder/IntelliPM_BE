using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.Enum.Task
{
    public enum TaskDependencyEnum
    {
        FINISH_START = 1,
        START_START = 2,
        FINISH_FINISH = 3,
        START_FINISH = 4
    }
}
