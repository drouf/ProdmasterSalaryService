using ProdmasterSalaryService.Database;
using ProdmasterSalaryService.Models.Classes;
using System.Linq.Expressions;

namespace ProdmasterSalaryService.Repositories
{
    public class OperationRepository : Repository<Operation, long, UserContext>
    {
        private readonly UserContext _dbContext;
        protected override Expression<Func<Operation, long>> Key => model => model.Id;
        public OperationRepository(UserContext ctx, UserContext dbContext) : base(dbContext,
            (appDbContext) => appDbContext.Operations)
        {
            _dbContext = dbContext;
        }
    }
}
