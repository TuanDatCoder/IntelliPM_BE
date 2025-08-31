using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.RefreshTokenRepos;

namespace IntelliPM.Services.JWTServices
{
    public class JWTService :IJWTService
    {
        private readonly IConfiguration _config;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        public JWTService(IConfiguration config, IRefreshTokenRepository refreshTokenRepository)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _tokenHandler = new JwtSecurityTokenHandler();
            _refreshTokenRepository = refreshTokenRepository;
        }
 
        public string decodeToken(string jwtToken, string nameClaim)
        {
            Claim? claim = _tokenHandler.ReadJwtToken(jwtToken).Claims.FirstOrDefault(selector => selector.Type.Equals(nameClaim));

            return claim != null ? claim.Value : "Error!!!"; 
        }


        public string GenerateJWT<T>(T entity) where T : class
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:JwtKey"]));
            var credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>();

            if (entity is Account account)
            {
 
                var accountRole = account.Role.ToString();  
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, accountRole)); 
                claims.Add(new Claim("accountId", account.Id.ToString()));
                claims.Add(new Claim("email", account.Email));
                claims.Add(new Claim("username", account.Username));
            }
            else
            {
                throw new ArgumentException("Unsupported entity type");
            }

            var token = new JwtSecurityToken(
               issuer: _config["JwtSettings:Issuer"],
               audience: _config["JwtSettings:Audience"],
               claims: claims,
               //expires: DateTime.Now.AddMonths(1),
               expires: DateTime.Now.AddDays(6),
               signingCredentials: credential
               );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodedToken;
        }


        public string GenerateRefreshToken()
        {
            var newRefreshToken = Guid.NewGuid().ToString();
            return newRefreshToken;
        }
    }
}
