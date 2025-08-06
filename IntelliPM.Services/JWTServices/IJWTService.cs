using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.JWTServices
{
    public interface IJWTService
    {
        string GenerateJWT<T>(T entity) where T : class;
        string GenerateRefreshToken();
        string decodeToken(string jwtToken, string nameClaim);
    }
}
