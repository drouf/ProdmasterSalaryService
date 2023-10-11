using ProdmasterSalaryService.Database;
using ProdmasterSalaryService.Models.Classes;
using System.Linq.Expressions;

namespace ProdmasterSalaryService.Repositories
{
    public class UserRepository : Repository<User, long, UserContext>
    {
        private readonly UserContext _dbContext;
        protected override Expression<Func<User, long>> Key => model => model.Id;
        public UserRepository(UserContext ctx, UserContext dbContext) : base(dbContext,
            (appDbContext) => appDbContext.User)
        {
            _dbContext = dbContext;
        }
    }
}
