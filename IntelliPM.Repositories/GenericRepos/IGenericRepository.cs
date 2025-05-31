using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.GenericRepos
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> Get(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "",
            int? pageIndex = null,
            int? pageSize = null);

        Task<T?> Get(int id);
        Task<List<T>> GetAll(int? page, int? size);
        Task<List<T>> GetAll();
        Task Add(T entity);
        Task Update(T entity);
        Task Remove(T entity);
        Task AddRange(List<T> entities);
        Task UpdateRange(List<T> entities);
        Task DeleteRange(List<T> entities);
    }
}
