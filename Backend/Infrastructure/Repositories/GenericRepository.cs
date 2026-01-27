using Domain.Interfaces;
using Domain.Specifications;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return null;
            
            // Filter out soft-deleted entities
            if (entity is ISoftDeletable softDeletable && !softDeletable.IsActive)
                return null;
                
            return entity;
        }
        
        public virtual async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            var query = _dbSet.AsQueryable();
            
            // Filter out soft-deleted entities if T implements ISoftDeletable
            if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
            {
                // Build expression: e => e.IsActive
                // Access IsActive property directly on the entity type
                var parameter = Expression.Parameter(typeof(T), "e");
                var property = typeof(T).GetProperty(nameof(ISoftDeletable.IsActive), BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    var propertyAccess = Expression.Property(parameter, property);
                    var lambda = Expression.Lambda<Func<T, bool>>(propertyAccess, parameter);
                    query = query.Where(lambda);
                }
            }
            
            // Order by CreatedAt descending if property exists
            var createdAtProperty = typeof(T).GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance);
            if (createdAtProperty != null)
            {
                var parameter = Expression.Parameter(typeof(T), "e");
                var propertyAccess = Expression.Property(parameter, createdAtProperty);
                var lambda = Expression.Lambda<Func<T, DateTime>>(propertyAccess, parameter);
                query = query.OrderByDescending(lambda);
            }
            
            return (await query.ToListAsync()).AsReadOnly();
        }

        public virtual async Task<IReadOnlyCollection<T>> FindAsync(ISpecification<T> specification)
        {
            var query = _dbSet.AsQueryable();

            // Filter out soft-deleted entities if T implements ISoftDeletable
            if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
            {
                // Build expression: e => e.IsActive
                // Access IsActive property directly on the entity type
                var parameter = Expression.Parameter(typeof(T), "e");
                var property = typeof(T).GetProperty(nameof(ISoftDeletable.IsActive), BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    var propertyAccess = Expression.Property(parameter, property);
                    var lambda = Expression.Lambda<Func<T, bool>>(propertyAccess, parameter);
                    query = query.Where(lambda);
                }
            }

            // Apply includes - EF Core's Include accepts Expression<Func<T, object?>>
            foreach (var include in specification.Includes)
            {
                query = EntityFrameworkQueryableExtensions.Include(query, include);
            }

            // Apply where clause
            var expression = specification.ToExpression();
            query = query.Where(expression);

            // Order by CreatedAt descending if property exists
            var createdAtProperty = typeof(T).GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance);
            if (createdAtProperty != null)
            {
                var parameter = Expression.Parameter(typeof(T), "e");
                var propertyAccess = Expression.Property(parameter, createdAtProperty);
                var lambda = Expression.Lambda<Func<T, DateTime>>(propertyAccess, parameter);
                query = query.OrderByDescending(lambda);
            }

            return (await query.ToListAsync()).AsReadOnly();
        }

        public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        
        public virtual async Task UpdateAsync(T entity)
        {
            await Task.CompletedTask;
            _dbSet.Update(entity);
        }
        
        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return;
            
            // Use soft delete if entity implements ISoftDeletable
            if (entity is ISoftDeletable softDeletable)
            {
                softDeletable.Deactivate();
                await UpdateAsync(entity);
            }
            else
            {
                // Hard delete for entities that don't support soft delete (e.g., AuditLog)
                _dbSet.Remove(entity);
            }
        }
        
        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return false;
            
            // Check if soft-deleted
            if (entity is ISoftDeletable softDeletable && !softDeletable.IsActive)
                return false;
                
            return true;
        }
    }
}
