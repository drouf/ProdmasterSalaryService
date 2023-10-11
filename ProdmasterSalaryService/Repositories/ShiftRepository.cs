using ProdmasterSalaryService.Database;
using ProdmasterSalaryService.Models.Classes;
using System.Linq.Expressions;

namespace ProdmasterSalaryService.Repositories
{
    public class ShiftRepository : Repository<Shift, long, UserContext>
    {
        private readonly UserContext _dbContext;
        protected override Expression<Func<Shift, long>> Key => model => model.Id;
        public ShiftRepository(UserContext ctx, UserContext dbContext) : base(dbContext,
            (appDbContext) => appDbContext.Shift)
        {
            _dbContext = dbContext;
        }
    }
}
