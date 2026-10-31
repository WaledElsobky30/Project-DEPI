using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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

    public IQueryable<T> Query()
    {
        return _db.AsQueryable();
    }

    public IEnumerable<T> GetAll()
    {
        return _db.ToList();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _db.ToListAsync();
    }

    public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return _db.Where(predicate).ToList();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _db.Where(predicate).ToListAsync();
    }

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        return _db.FirstOrDefault(predicate);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _db.FirstOrDefaultAsync(predicate);
    }

    public bool Any(Expression<Func<T, bool>> predicate)
    {
        return _db.Any(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _db.AnyAsync(predicate);
    }

    public int Count(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate == null ? _db.Count() : _db.Count(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate == null ? await _db.CountAsync() : await _db.CountAsync(predicate);
    }

    public T? GetById(int id)
    {
        return _db.Find(id);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _db.FindAsync(id);
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

    public void RemoveRange(IEnumerable<T> entities)
    {
        _db.RemoveRange(entities);
    }

    public int Save()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
