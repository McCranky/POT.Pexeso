using Microsoft.EntityFrameworkCore;
using POT.Pexeso.Server.Data.Models;

namespace POT.Pexeso.Data
{
    public class PexesoDataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Card> Cards { get; set; }

        private static bool initialized = false; // unlogs all users on startup
        public PexesoDataContext(DbContextOptions<PexesoDataContext> options) : base(options) {
            if (!initialized) {
                initialized = true;
                foreach (var user in Users) {
                    user.IsOnline = false;
                    Users.Update(user);
                }
                SaveChanges();
            }
        }

    }
}
