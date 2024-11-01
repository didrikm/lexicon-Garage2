using System;
using lexicon_Garage2.Models;
using Microsoft.EntityFrameworkCore;

namespace lexicon_Garage2.Data
{
    public class lexicon_Garage2Context : DbContext
    {
        public lexicon_Garage2Context(DbContextOptions<lexicon_Garage2Context> options)
            : base(options) { }

        public DbSet<Vehicle> Vehicle { get; set; } = default!;
        public DbSet<ParkingSpot> ParkingSpot { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vehicle>().HasIndex(v => v.RegistrationNumber).IsUnique();

            modelBuilder.Entity<ParkingSpot>().Property(ps => ps.IsAvailable).HasDefaultValue(true);
        }
    }
}
