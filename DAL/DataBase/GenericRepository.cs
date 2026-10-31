using BookifyHotel.Data;
using DAL.DataBase;
using Microsoft.EntityFrameworkCore;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly BookifyHotelDbContext _context;
    private readonly DbSet<T> _db;

    public GenericRepository(BookifyHotelDbContext context)
    {
        _context = context;
        _db = _context.Set<T>();    // Initialize the DbSet for the entity type T example: instead of context.Users 
    }

    public IEnumerable<T> GetAll()
    {
        return _db.ToList();
    }

    public T GetById(int id)
    {
        return _db.Find(id);
    }

    public void Add(T entity)
    {
        _db.Add(entity);
    }

    public void Update(T entity)
    {
        _db.Update(entity);
    }

    public void Delete(int id)
    {
        var entity = _db.Find(id);
        if (entity != null)
        {
            _db.Remove(entity);
        }
    }

    public int Save()
    {
        return _context.SaveChanges();
    }
}
