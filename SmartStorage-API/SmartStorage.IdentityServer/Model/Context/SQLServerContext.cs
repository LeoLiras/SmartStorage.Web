using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SmartStorage.IdentityServer.Model.Context
{
    public class SQLServerContext : IdentityDbContext<ApplicationUser>
    {
        public SQLServerContext(DbContextOptions<SQLServerContext> options)
            : base(options) { }
    }
}
