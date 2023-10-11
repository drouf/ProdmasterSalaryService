using ProdmasterSalaryService.Database;
using ProdmasterSalaryService.Models.Classes;
using System.Linq.Expressions;

namespace ProdmasterSalaryService.Repositories
{
    public class CustomRepository : Repository<Custom, long, UserContext>
    {
        private readonly UserContext _dbContext;
        protected override Expression<Func<Custom, long>> Key => model => model.Id;
        public CustomRepository(UserContext ctx, UserContext dbContext) : base(dbContext,
            (appDbContext) => appDbContext.Custom)
        {
            _dbContext = dbContext;
        }
    }
}
