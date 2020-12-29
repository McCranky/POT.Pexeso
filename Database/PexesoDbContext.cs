using Database.Models;
using Microsoft.EntityFrameworkCore;
using POT.Pexeso.Shared;

namespace Database
{
    public class PexesoDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<CardBackInfo> Cards { get; set; }
        public DbSet<GameRecord> Records { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseSqlite("Data Source = ../SharedWorkingDirectory/PexesoData.db");
            }
        }

    }
}
