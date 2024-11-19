using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public DbSet<Vehicle> Vehicle { get; set; } = default!;
        public DbSet<ParkingSpot> ParkingSpots { get; set; } = default!;
        public DbSet<VehicleType> VehicleTypes { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.RegistrationNumber)
                .IsUnique();
        }
    }
}
