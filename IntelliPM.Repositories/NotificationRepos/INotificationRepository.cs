using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.NotificationRepos
{
    public interface INotificationRepository
    {
        Task Add(Notification notification);
    }
}
