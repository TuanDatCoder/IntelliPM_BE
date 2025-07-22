using IntelliPM.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.GenericRepos
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly Su25Sep490IntelliPmContext context;
        private readonly DbSet<T> _entities;

        public GenericRepository(Su25Sep490IntelliPmContext context)
        {
            this.context = context;
            _entities = context.Set<T>();

        }
        public async Task AddRange(List<T> entities)
        {
            try
            {
                _entities.AddRangeAsync(entities);
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteRange(List<T> entities)
        {
            try
            {
                _entities.RemoveRange(entities);
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<T?> Get(int id)
        {
            try
            {
                return await _entities.FindAsync(id);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<T>> GetAll(int? page, int? size)
        {
            try
            {

                if (page.HasValue && size.HasValue)
                {
                    int validPage = page.Value > 0 ? page.Value : 1;
                    int validSize = size.Value > 0 ? size.Value : 10;

                    return await _entities.Skip((validPage - 1) * validSize).Take(validSize).ToListAsync();
                }

                return await _entities.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task Add(T entity)
        {
            try
            {
                await _entities.AddAsync(entity);
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task Remove(T entity)
        {
            try
            {
                _entities.Remove(entity);
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task Update(T entity)
        {
            try
            {
                _entities.Update(entity);
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateRange(List<T> entities)
        {
            try
            {
                _entities.UpdateRange(entities);
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<T>> Get(Expression<Func<T,
            bool>>? filter = null,
            Func<IQueryable<T>,
            IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "",
            int? pageIndex = null, int? pageSize = null)
        {
            IQueryable<T> query = _entities;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
                int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10;

                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            return await query.ToListAsync();
        }

        public async Task<List<T>> GetAll()
        {
            try
            {
                return await _entities.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
