using System.ComponentModel.DataAnnotations;

namespace lexicon_Garage2.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required]
        public VehicleType VehicleType { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string RegistrationNumber { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Color { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Brand { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Model { get; set; }

        [Required]
        [Range(0, 20)]
        public int NumberOfWheels { get; set; }

        public DateTime ParkingTime { get; set; }
        public ParkingSpot? ParkingSpot { get; set; }

        public Vehicle()
        {
            ParkingTime = DateTime.Now;
        }
    }

    public enum VehicleType
    {
        Car,
        Bus,
        Airplane,
        Boat,
        Motorcycle,
    }
}
