using AutoMapper;
using IntelliPM.Data.DTOs.RecipientNotification.Response;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Repositories.NotificationRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.RecipientNotificationRepos;
using IntelliPM.Repositories.SubtaskCommentRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Services.SubtaskCommentServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RecipientNotificationServices
{
    public class RecipientNotificationService : IRecipientNotificationService
    {
        private readonly IMapper _mapper;
        private readonly IRecipientNotificationRepository _recipientNotificationRepository;

        public RecipientNotificationService(IMapper mapper, IRecipientNotificationRepository recipientNotificationRepository)
        {
            _mapper = mapper;
            _recipientNotificationRepository = recipientNotificationRepository;
        }
        public async Task<List<RecipientNotificationResponseDTO>> GetAllRecipientNotification()
        {
            var entities = await _recipientNotificationRepository.GetAllRecipientNotification();
            return _mapper.Map<List<RecipientNotificationResponseDTO>>(entities);
        }
    }
}
