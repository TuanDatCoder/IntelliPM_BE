
using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.GenericRepos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.RefreshTokenRepos
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public RefreshTokenRepository(Su25Sep490IntelliPmContext context) : base(context)
        {
            _context = context;
        }

        public async Task<RefreshToken> GetByRefreshToken(string refreshToken)
        {
            return await _context.RefreshToken
                .Include(x => x.Account)
                .Where(x => x.Token.Equals(refreshToken)).FirstOrDefaultAsync();
        }
    }
}
