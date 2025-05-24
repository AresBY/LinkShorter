using LinkShorter.Data.Models;
using LinkShorter.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LinkShorter.Data.Repositories.Implementations
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseDl
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _entities;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _entities = context.Set<T>();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _entities.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _entities.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetPagedDataAsync(int page, int pageSize)
        {
            return await _entities.OrderByDescending(u => u.Id)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize).ToListAsync();
        }

        public async Task<bool> AddAsync(T entity)
        {
            await _entities.AddAsync(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            _entities.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _entities.Remove(entity);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}

