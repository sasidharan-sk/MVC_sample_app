using Microsoft.EntityFrameworkCore;
using MVC_sample_app.Models.Domain;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace MVC_sample_app.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
    }

}
