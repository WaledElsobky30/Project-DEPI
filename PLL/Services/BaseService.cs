using DAL.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLL.Services
{
    /*
     ده Template جاهز
• هنورثه في كل Service تاني
• علشان نقلّل التكرار ونشتغل OOP صح
     */
    public class BaseService<T> where T : class
    {
        protected readonly IGenericRepository<T> _repo;

        public BaseService(IGenericRepository<T> repo)
        {
            _repo = repo;
        }

        public IEnumerable<T> GetAll()
        {
            return _repo.GetAll();
        }

        public Task<List<T>> GetAllAsync()
        {
            return _repo.GetAllAsync();
        }

        public T? GetById(int id)
        {
            return _repo.GetById(id);
        }

        public Task<T?> GetByIdAsync(int id)
        {
            return _repo.GetByIdAsync(id);
        }

        public void Create(T entity)
        {
            _repo.Add(entity);
            _repo.Save();
        }

        public async Task CreateAsync(T entity)
        {
            _repo.Add(entity);
            await _repo.SaveAsync();
        }

        public void Update(T entity)
        {
            _repo.Update(entity);
            _repo.Save();
        }

        public async Task UpdateAsync(T entity)
        {
            _repo.Update(entity);
            await _repo.SaveAsync();
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
            _repo.Save();
        }

        public async Task DeleteAsync(int id)
        {
            _repo.Delete(id);
            await _repo.SaveAsync();
        }
    }
}
