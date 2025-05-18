using Microsoft.EntityFrameworkCore;
using Nalix.Common.Repositories.Async;
using Nalix.Common.Repositories.Sync;
using Nalix.Game.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nalix.Game.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation using Entity Framework Core.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public class Repository<T>(GameDbContext context) : IRepository<T>, IRepositoryAsync<T> where T : class
{
    private readonly GameDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly DbSet<T> _dbSet = context.Set<T>();

    // ================================
    // Synchronous Methods
    // ================================

    public IEnumerable<T> GetAll(int pageNumber = 1, int pageSize = 10)
        => [.. _dbSet.AsNoTracking()
                 .Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize)];

    public int Count() => _dbSet.Count();

    public bool Exists(int id) => _dbSet.Find(id) != null;

    public T GetFirstOrDefault(Expression<Func<T, bool>> predicate) => _dbSet.FirstOrDefault(predicate);

    public bool Any(Expression<Func<T, bool>> predicate) => _dbSet.Any(predicate);

    public T GetById(int id) => _dbSet.Find(id);

    public IEnumerable<T> Find(
        Expression<Func<T, bool>> predicate,
        int pageNumber = 1, int pageSize = 10)
        => [.. _dbSet.AsNoTracking()
                 .Where(predicate)
                 .Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize)];

    public IEnumerable<T> Get(Expression<Func<T, bool>> filter = null,
                              Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                              string includeProperties = "",
                              int pageNumber = 1, int pageSize = 10)
    {
        var query = _dbSet.AsQueryable();
        if (filter is not null) query = query.Where(filter);

        foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
            query = query.Include(prop.Trim());

        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        return orderBy is not null ? [.. orderBy(query)] : query.ToList();
    }

    public void Add(T entity) => _dbSet.Add(entity);

    public void AddRange(IEnumerable<T> entities) => _dbSet.AddRange(entities);

    public void Update(T entity) => _dbSet.Update(entity);

    public void UpdateRange(IEnumerable<T> entities) => _dbSet.UpdateRange(entities);

    public void Delete(int id) => _dbSet.Remove(_dbSet.Find(id)!);

    public void Delete(T entity) => _dbSet.Remove(entity);

    public void DeleteRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

    public void Detach(T entity) => _context.Entry(entity).State = EntityState.Detached;

    public IQueryable<T> AsQueryable() => _dbSet.AsQueryable();

    public int SaveChanges() => _context.SaveChanges();

    // ================================
    // Asynchronous Methods
    // ================================

    public async Task<IEnumerable<T>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
                       .Skip((pageNumber - 1) * pageSize)
                       .Take(pageSize)
                       .ToListAsync(cancellationToken);

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(cancellationToken);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(predicate, cancellationToken);

    public async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([id], cancellationToken);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
                       .Where(predicate)
                       .Skip((pageNumber - 1) * pageSize)
                       .Take(pageSize)
                       .ToListAsync(cancellationToken);

    public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter = null,
                                               Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                               string includeProperties = "",
                                               int pageNumber = 1, int pageSize = 10,
                                               CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        if (filter is not null) query = query.Where(filter);

        foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
            query = query.Include(prop.Trim());

        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        return orderBy is not null ? await orderBy(query).ToListAsync(cancellationToken)
                                   : await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        => await _dbSet.AddRangeAsync(entities, cancellationToken);

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => _dbSet.Remove(await _dbSet.FindAsync([id], cancellationToken)!);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([id], cancellationToken) != null;

    public async Task<T> GetFirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
}