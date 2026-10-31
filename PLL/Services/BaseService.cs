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

        public T GetById(int id)
        {
            return _repo.GetById(id);
        }

        public void Create(T entity)
        {
            _repo.Add(entity);
            _repo.Save();
        }

        public void Update(T entity)
        {
            _repo.Update(entity);
            _repo.Save();
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
            _repo.Save();
        }
    }

}
