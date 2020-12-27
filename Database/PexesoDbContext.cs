using Database.Models;
using Microsoft.EntityFrameworkCore;
using POT.Pexeso.Shared;
using POT.Pexeso.Shared.Pexeso;
using System;

namespace Database
{
    public class PexesoDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<CardBackInfo> Cards { get; set; }
        public DbSet<GameRecord> Records { get; set; }
        //public DbSet<Moves> Moves { get; set; }

        private static bool initialized = false; // unlogs all users on startup
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseSqlite("Data Source = ../SharedWorkingDirectory/PexesoData.db");
            }

            //if (!initialized) {
            //    initialized = true;
            //    foreach (var user in Users) {
            //        user.IsOnline = false;
            //        Users.Update(user);
            //    }
            //    SaveChanges();
            //}
        }
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    //modelBuilder.Entity<MoveRecord>(entity => {
        //    //    entity.ToTable("Records");

        //    //    entity.Property(p => p.Moves).
        //    //});
        //    modelBuilder.Entity<MoveRecord>().HasNoKey();

        //    base.OnModelCreating(modelBuilder);
        //}
    }
}
