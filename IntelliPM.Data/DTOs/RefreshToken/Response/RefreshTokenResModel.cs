using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.RefreshToken.Response
{
    public class RefreshTokenResModel
    {
        public string accessToken { get; set; }
        public string newRefreshToken { get; set; }
    }
}
