using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore;
using NPO_Workflow.DAL.Models;
namespace NPO_Workflow.DAL
{
    public class NPOContext : DbContext
    {
        public DbSet<Operation> Operations { get; set; }
        public NPOContext(DbContextOptions<NPOContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //TBD
        }
    }
}
