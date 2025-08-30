using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.Enum.ActivityLogRelatedEntityType
{
    public enum ActivityLogRelatedEntityTypeEnum
    {
        TASK = 1,
        PROJECT = 2,
        COMMENT = 3,
        FILE = 4,
        NOTIFICATION = 5,
        RISK = 6,
        EPIC=7,

        SUBTASK=8,
        SUBTASK_FILE = 9,
        SUBTASK_COMMENT = 10,
        MILESTONE = 11,

    }
}
