using Microsoft.EntityFrameworkCore;
using lexicon_Garage2.Models;

namespace lexicon_Garage2.Data
{
    public class lexicon_Garage2Context : DbContext
    {
        public lexicon_Garage2Context(DbContextOptions<lexicon_Garage2Context> options)
            : base(options)
        {
        }

        public DbSet<lexicon_Garage2.Models.Vehicle> Vehicle { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.RegistrationNumber)
                .IsUnique();
        }
    }
}
