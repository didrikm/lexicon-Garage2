using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lexicon_Garage2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace lexicon_Garage2.Data
{
    public class lexicon_Garage2Context : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public lexicon_Garage2Context(DbContextOptions<lexicon_Garage2Context> options)
            : base(options) { }

        public DbSet<lexicon_Garage2.Models.Vehicle> Vehicle { get; set; } = default!;
        public DbSet<ParkingSpot> ParkingSpots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vehicle>().HasIndex(v => v.RegistrationNumber).IsUnique();

            modelBuilder.Entity<ApplicationUser>().HasIndex(u => u.Id).IsUnique();
            // Konfigurera relationen mellan Vehicle och ParkingSpot
            modelBuilder.Entity<ParkingSpot>()
                .HasOne(p => p.Vehicle) // En ParkingSpot kan ha ett fordon
                .WithOne(v => v.ParkingSpot) // Ett fordon har exakt en ParkingSpot
                .HasForeignKey<ParkingSpot>(p => p.VehicleId); // Sätt VehicleId som foreign key
        }
    }
}
