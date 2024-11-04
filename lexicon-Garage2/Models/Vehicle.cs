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

        public Vehicle()
        {
            ParkingTime = DateTime.Now;
        }

        public double Size
        {
            get 
            {
                switch (VehicleType)
                {
                    case VehicleType.Car:
                        return 1;
                    case VehicleType.Bus:
                        return 2;
                    case VehicleType.Airplane:
                        return 5;
                    case VehicleType.Boat:
                        return 3;
                    case VehicleType.Motorcycle:
                        return 1;
                    default:
                        return 0;
                }
            }
        }

        public static double GetSizeOfVehicleType(VehicleType type)
        {
            switch (type)
            {
                case VehicleType.Car:
                    return 1;
                case VehicleType.Bus:
                    return 2;
                case VehicleType.Airplane:
                    return 5;
                case VehicleType.Boat:
                    return 3;
                case VehicleType.Motorcycle:
                    return 1;
                default:
                    return 0;
            }
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
