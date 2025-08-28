using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ShareServices
{
    public class ShareTokenService : IShareTokenService
    {

        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;

        public ShareTokenService(IConfiguration configuration)
        {
            var secretKey = configuration["JwtSettings:JwtKey"];
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            _issuer = configuration["JwtSettings:Issuer"];
        }

        public string GenerateShareToken(int documentId, int accountId, string permissionType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _issuer,
                Audience = _issuer,
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("docId", documentId.ToString()),
                new Claim("accId", accountId.ToString()),
                new Claim("perm", permissionType)
            }),
            
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public (int DocumentId, int AccountId, string PermissionType)? ValidateShareToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _key,
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _issuer,
                    ClockSkew = TimeSpan.Zero // Không cho phép chênh lệch thời gian
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var documentId = int.Parse(jwtToken.Claims.First(x => x.Type == "docId").Value);
                var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "accId").Value);
                var permissionType = jwtToken.Claims.First(x => x.Type == "perm").Value;

                return (documentId, accountId, permissionType);
            }
            catch
            {
                
                return null;
            }
        }
    }
}
