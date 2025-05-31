
using IntelliPM.Data.DTOs.TokenModels;
using IntelliPM.Services.JWTServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.Helper.DecodeTokenHandler
{
    public class DecodeTokenHandler : IDecodeTokenHandler
    {
        private readonly IJWTService _jWTService;

        public DecodeTokenHandler(IJWTService jWTService)
        {
            _jWTService = jWTService;
        }
        //public TokenModel decode(string token)
        //{
        //    var roleName = _jWTService.decodeToken(token, ClaimsIdentity.DefaultRoleClaimType);
        //    var userId = _jWTService.decodeToken(token, "userid");
        //    var email = _jWTService.decodeToken(token, "email");
        //    var username = _jWTService.decodeToken(token, "username");

        //    return new TokenModel(userId, roleName, email, username);
        //}
        public TokenModel decode(string token)
        {
            var roleName = _jWTService.decodeToken(token, ClaimsIdentity.DefaultRoleClaimType); // Giải mã vai trò từ token
            var userId = _jWTService.decodeToken(token, "accountId"); // Giải mã ID người dùng từ token
            var email = _jWTService.decodeToken(token, "email");
            var username = _jWTService.decodeToken(token, "username");

            return new TokenModel(userId, roleName, email, username);
        }

    }
}
