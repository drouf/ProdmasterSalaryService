using Microsoft.EntityFrameworkCore;
using ProdmasterSalaryService.Models.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace ProdmasterSalaryService.Repositories
{
    public abstract class Repository<TModel, TKey, TDbContext> where TModel : class, IDisanModel
        where TDbContext : DbContext
    {
        protected readonly DbSet<TModel> _dBSet;
        private readonly TDbContext _dbContext;

        protected abstract Expression<Func<TModel, TKey>> Key { get; }

        protected Repository(TDbContext dbContext, Func<TDbContext, DbSet<TModel>> dBSet)
        {
            _dbContext = dbContext;
            _dBSet = dBSet(dbContext);
        }

        public virtual async Task<TModel[]> Select()
        {
            return await _dBSet.ToArrayAsync();
        }

        public async Task<TModel[]> OrderBy(Expression<Func<TModel, TKey>> expression)
        {
            return await _dBSet.OrderBy(expression).ToArrayAsync();
        }

        public async Task<TModel[]> OrderByDescending(Expression<Func<TModel, TKey>> expression)
        {
            return await _dBSet.OrderByDescending(expression).ToArrayAsync();
        }

        public async Task<TModel[]> Paginate(int offset, int limit)
        {
            return await _dBSet.OrderBy(Key).Skip(offset).Take(limit).ToArrayAsync();
        }

        public virtual async Task<TModel[]> Select(Expression<Func<TModel, bool>> predicate)
        {
            return await _dBSet.Where(predicate).ToArrayAsync();
        }

        public virtual async Task<TModel> First(Expression<Func<TModel, bool>> predicate)
        {
            return await _dBSet.Where(predicate).FirstOrDefaultAsync();
        }

        public async Task<TModel> Add(TModel model)
        {
            try
            {
                await _dBSet.AddAsync(model);
                await _dbContext.SaveChangesAsync();
                return model;
            }
            catch (DbUpdateException exception)
            {
                return null;
            }
        }

        public async Task AddRange(IEnumerable<TModel> range)
        {
            try
            {
                await _dBSet.AddRangeAsync(range);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception) { }
        }

        public async Task Remove(TModel model)
        {
            try
            {
                _dbContext.Entry<TModel>(model).State = EntityState.Deleted;
                _dbContext.SaveChanges();
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                return;
            }
        }

        public async Task<TModel> Update(TModel entity)
        {
            try
            {
                _dBSet.Update(entity);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exception)
            {
                await exception.Entries.Single().ReloadAsync();
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return null;
            }
            return entity;
        }

        public async Task<TModel> Update(TModel entity, IEnumerable<string> updatedParams)
        {
            try
            {
                var model = await First(e => e.DisanId == entity.DisanId);
                if (model == null)
                {
                    throw new Exception("no entity");
                }

                foreach(var param in updatedParams)
                {
                    if (entity.GetType().GetProperty(param) != null && model.GetType().GetProperty(param) != null)
                    {
                        model.GetType().GetProperty(param).SetValue(model, entity.GetType().GetProperty(param).GetValue(entity));
                    }
                }
                
                _dBSet.Update(model);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exception)
            {
                await exception.Entries.Single().ReloadAsync();
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
            return entity;
        }

        public async Task AddOrUpdateRange(IEnumerable<TModel> range)
        {
            try
            {
                foreach (TModel model in range)
                {
                    var duplicate = await _dBSet
                        .Where<TModel>(m => m.DisanId == model.DisanId)
                        .FirstOrDefaultAsync();
                    if (duplicate == null)
                    {
                        await Update(model);
                    }
                    else
                    {
                        await Add(model);
                    }
                }
            }
            catch (DbUpdateException exception) { }
        }

        public async Task AddOrUpdateRange(IEnumerable<TModel> range, IEnumerable<string> updatedParams)
        {
            try
            {
                foreach (TModel model in range)
                {
                    var duplicate = await _dBSet
                        .Where<TModel>(m => m.DisanId == model.DisanId)
                        .FirstOrDefaultAsync();
                    if (duplicate == null)
                    {
                        await Add(model);
                    }
                    else
                    {
                        await Update(model, updatedParams);   
                    }
                }
            }
            catch (DbUpdateException exception) { }
            catch (Exception ex) { var msg = ex.Message; }
        }

        public async Task<TModel[]> Where(Expression<Func<TModel, bool>> predicate)
        {
            return await _dBSet.Where(predicate).ToArrayAsync();
        }

        public async Task<long> Count()
        {
            return await _dBSet.LongCountAsync();
        }
    }
}
