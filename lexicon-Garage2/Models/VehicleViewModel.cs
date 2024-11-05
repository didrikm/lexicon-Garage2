namespace lexicon_Garage2.Models
{
    public class VehicleViewModel
    {
        public VehicleViewModel(Vehicle vehicle)
        {
            Id = vehicle.Id;
            VehicleType = vehicle.VehicleType;
            RegistrationNumber = vehicle.RegistrationNumber;
            ArrivalTime = vehicle.ParkingTime;
            ParkingSpotNumber = vehicle.ParkingSpot;
        }

        public int Id { get; set; }
        public VehicleType VehicleType { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int ParkingSpot { get; set; }
        public int? ParkingSpotNumber { get; set; }
    }
}
