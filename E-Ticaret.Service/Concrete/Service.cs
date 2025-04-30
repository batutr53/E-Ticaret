using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace E_Ticaret.Service.Concrete
{
    public class Service<T> : IService<T> where T : class, IEntity, new()
    {
        private DatabaseContext _context;
        private DbSet<T> _dbSet;

        public Service(DatabaseContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public T Find(int id)
        {
            return _dbSet.Find(id);
        }

        public async Task<T> FindAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public T Get(Expression<Func<T, bool>> expression)
        {
            return _dbSet.FirstOrDefault(expression);
        }

        public List<T> GetAll()
        {
            return _dbSet.AsNoTracking().ToList();
        }

        public List<T> GetAll(Expression<Func<T, bool>> expression)
        {
            return _dbSet.Where(expression).AsNoTracking().ToList();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.Where(expression).AsNoTracking().ToListAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.FirstOrDefaultAsync(expression);
        }

        public IQueryable<T> GetQueryable()
        {
            return _dbSet;
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }
        public async Task<int> SaveChangesAsync()
        {
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    foreach (var property in entry.Properties)
                    {
                        var value = property.CurrentValue;

                        // 1. DateTime (non-nullable)
                        if (value is DateTime dt && dt.Kind != DateTimeKind.Utc)
                        {
                            property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                        }

                        // 2. DateTime? (nullable)
                        else if (value is DateTime dt2)
                        {
                            if (dt2.Kind != DateTimeKind.Utc)
                            {
                                property.CurrentValue = DateTime.SpecifyKind(dt2, DateTimeKind.Utc);
                            }
                        }

                        // 3. List<DateTime>
                        else if (value is IEnumerable<DateTime> dtList)
                        {
                            property.CurrentValue = dtList
                                .Select(d => d.Kind == DateTimeKind.Utc ? d : DateTime.SpecifyKind(d, DateTimeKind.Utc))
                                .ToList();
                        }

                        // 4. List<DateTime?> ve benzeri IEnumerable<object> koleksiyonlar
                        else if (value is IEnumerable<object> objList)
                        {
                            var converted = objList.Select(item =>
                            {
                                if (item is DateTime dtItem && dtItem.Kind != DateTimeKind.Utc)
                                    return (object)DateTime.SpecifyKind(dtItem, DateTimeKind.Utc);

                                var nullableDate = item as DateTime?;
                                if (nullableDate.HasValue && nullableDate.Value.Kind != DateTimeKind.Utc)
                                    return (object?)DateTime.SpecifyKind(nullableDate.Value, DateTimeKind.Utc);

                                return item;
                            }).ToList();

                            property.CurrentValue = converted;
                        }
                    }
                }
            }

            return await _context.SaveChangesAsync();
        }



        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}
