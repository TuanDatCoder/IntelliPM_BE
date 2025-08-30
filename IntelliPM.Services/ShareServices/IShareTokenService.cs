using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ShareServices
{
    public interface IShareTokenService
    {

        string GenerateShareToken(int documentId, int accountId, string permissionType);

        (int DocumentId, int AccountId, string PermissionType)? ValidateShareToken(string token);
    }
}
