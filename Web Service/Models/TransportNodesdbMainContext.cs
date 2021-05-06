using Microsoft.EntityFrameworkCore;

namespace Web_Service.Models
{
    public class TransportNodesdbMainContext : DbContext
    {
        public TransportNodesdbMainContext()
        {
        }

        public DbSet<TransportNodeInfo> TransportNodes { get; set; }

        public TransportNodesdbMainContext(DbContextOptions<TransportNodesdbMainContext> options)
            : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();           
        }
    }
}