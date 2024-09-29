using Jadyn.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Jadyn.DataAccess
{
    public class JadynDbContext : DbContext
    {
        public JadynDbContext(DbContextOptions options) : base(options)
        {
            if (Database.GetMigrations().Count() != Database.GetAppliedMigrations().Count())
            {
                Database.Migrate();
            }
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<City> Cities { get; set; }
    }
}
