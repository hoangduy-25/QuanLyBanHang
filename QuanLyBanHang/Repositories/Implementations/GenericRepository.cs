using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using QuanLyBanHang.Data;
using QuanLyBanHang.Repositories.Interfaces;

namespace QuanLyBanHang.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly QuanLyBanHangDbContext Context;
        protected readonly DbSet<T> DbSet;

        public GenericRepository(QuanLyBanHangDbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        public virtual IQueryable<T> Query(bool asNoTracking = false)
        {
            return asNoTracking ? DbSet.AsNoTracking() : DbSet;
        }

        public virtual Task<T> GetByIdAsync(params object[] keyValues)
        {
            return DbSet.FindAsync(keyValues);
        }

        public virtual Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return DbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate).ToListAsync();
        }

        public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return DbSet.AnyAsync(predicate);
        }

        public virtual Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            return predicate == null ? DbSet.CountAsync() : DbSet.CountAsync(predicate);
        }

        public virtual void Add(T entity)
        {
            DbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            DbSet.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Remove(T entity)
        {
            DbSet.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            DbSet.RemoveRange(entities);
        }
    }
}
