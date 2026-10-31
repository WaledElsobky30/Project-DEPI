using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DAL.DataBase
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> Query();

        IEnumerable<T> GetAll();
        Task<List<T>> GetAllAsync();

        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

        T? FirstOrDefault(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        bool Any(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        int Count(Expression<Func<T, bool>>? predicate = null);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        T? GetById(int id);
        Task<T?> GetByIdAsync(int id);

        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
        void RemoveRange(IEnumerable<T> entities);

        int Save();
        Task<int> SaveAsync();
    }
}
