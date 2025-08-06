using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TokenModels
{
    public class TokenModel
    {

        public string accountId { get; set; }

        public string roleName { get; set; }

        public string email { get; set; }

        public string username { get; set; }

        public TokenModel(string accountId, string roleName, string email, string username)
        {
            this.accountId = accountId;
            this.roleName = roleName;
            this.email = email;
            this.username = username;
        }
    }
}
