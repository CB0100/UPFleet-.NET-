using System.Data.Entity;
using System.Reflection.Emit;
using System.Xml;
using UPFleet.Models;

namespace UPFleet.Context
{
    public class ApplicationDbContext : DbContext
    {
        //DB Context Class
        public ApplicationDbContext() : base("name=DbConnectionString")
        {
        }
        public DbSet<Barge> Barges { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<PeachtreeExportedArchive> peachtreeExportedArchives { get; set; }
        
    }
}