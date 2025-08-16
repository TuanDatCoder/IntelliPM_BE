using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntelliPM.Shared.Hubs
{
    public class DocumentHub : Hub
    {
        public async Task JoinDocumentGroup(int documentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"document-{documentId}");
        }

        public async Task LeaveDocumentGroup(int documentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"document-{documentId}");
        }
    }
}
