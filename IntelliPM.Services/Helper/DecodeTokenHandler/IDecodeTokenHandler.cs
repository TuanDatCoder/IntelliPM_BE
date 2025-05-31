using IntelliPM.Data.DTOs.TokenModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.Helper.DecodeTokenHandler
{
    public interface IDecodeTokenHandler
    {
        TokenModel decode(string token);

    }
}
