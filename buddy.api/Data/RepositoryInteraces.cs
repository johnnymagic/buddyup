using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BuddyUp.API.Data.Repositories
{
    /// <summary>
    /// Generic repository interface for database operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Get a queryable that can be used to query the entity
        /// </summary>
        IQueryable<T> Query();
        
        /// <summary>
        /// Get entity by ID
        /// </summary>
        Task<T> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Get entities by a predicate
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null);
        
        /// <summary>
        /// Add a new entity
        /// </summary>
        Task AddAsync(T entity);
        
        /// <summary>
        /// Add multiple entities
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities);
        
        /// <summary>
        /// Update an existing entity
        /// </summary>
        Task UpdateAsync(T entity);
        
        /// <summary>
        /// Remove an entity
        /// </summary>
        Task DeleteAsync(T entity);
        
        /// <summary>
        /// Remove multiple entities
        /// </summary>
        Task DeleteRangeAsync(IEnumerable<T> entities);
        
        /// <summary>
        /// Check if any entity matches the predicate
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        
        /// <summary>
        /// Count entities matching the predicate
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
        
        /// <summary>
        /// Save changes to the database
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}