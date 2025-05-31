using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IntelliPM.Repositories.GenericRepos;
using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;

namespace IntelliPM.Repositories.AccountRepos
{
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public AccountRepository(Su25Sep490IntelliPmContext context) : base(context)
        {
            _context = context;
        }


        public async Task<List<Account>> GetAccounts(int? page, int? size)
        {
            try
            {
                var pageIndex = (page.HasValue && page > 0) ? page.Value : 1;
                var sizeIndex = (size.HasValue && size > 0) ? size.Value : 10;

                return await _context.Account
                    .Skip((pageIndex - 1) * sizeIndex)
                    .Take(sizeIndex)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<Account> GetAccountById(int accountId)
        {
            return await _context.Account.FirstOrDefaultAsync(a => a.Id == accountId);
        }
        public async Task<int> AddAccount(Account account)
        {
            try
            {
                await _context.Account.AddAsync(account);
                await _context.SaveChangesAsync();

                return account.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving account: {ex.InnerException?.Message ?? ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Account> GetAccountByEmail(string email)
        {
            try
            {
                return await _context.Account.FirstOrDefaultAsync(x => x.Email.Equals(email));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Account> GetAccountByUsername(string username)
        {
            try
            {
                return await _context.Account.FirstOrDefaultAsync(x => x.Username.Equals(username));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> IsExistedByEmail(string email)
        {
            try
            {
                return await _context.Account.FirstOrDefaultAsync(x => x.Email.Equals(email)) != null ? true : false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> IsExistedByUsername(string username)
        {
            try
            {
                return await _context.Account.FirstOrDefaultAsync(x => x.Username.Equals(username)) != null ? true : false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Account> Update(Account account)
        {
            try
            {
                _context.Account.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error when updating account: {ex.InnerException?.Message ?? ex.Message}");
            }

        }

        public async Task<int> GetTotalUsersAsync()
        {
            return await _context.Account.CountAsync(); 
        }
    }
}
