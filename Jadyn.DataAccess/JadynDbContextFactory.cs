using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jadyn.DataAccess
{
    public class JadynDbContextFactory : IDesignTimeDbContextFactory<JadynDbContext>
    {
        public JadynDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<JadynDbContext>();

            optionsBuilder.UseSqlite($"Data Source = jadyn.db");

            return new JadynDbContext(optionsBuilder.Options);
        }
    }
}
