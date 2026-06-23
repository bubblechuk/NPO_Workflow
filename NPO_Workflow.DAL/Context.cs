using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore;
namespace NPO_Workflow.DAL
{
    public class NPOContext : DbContext
    {
        public NPOContext(DbContextOptions<NPOContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //TBD
        }
    }
}
