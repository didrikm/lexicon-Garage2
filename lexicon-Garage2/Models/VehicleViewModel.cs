namespace lexicon_Garage2.Models
{
    public class VehicleViewModel
    {
        public VehicleViewModel(Vehicle vehicle)
        {
            Id = vehicle.Id;
            RegistrationNumber = vehicle.RegistrationNumber;
            ArrivalTime = vehicle.ParkingTime;
            ParkingSpotsTaken = vehicle.ParkingSpots.Count; 
        }

        public int Id { get; set; }
        public string VehicleType { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime ArrivalTime { get; set; }
        //public int ParkingSpot { get; set; }
        public int? ParkingSpotsTaken { get; set; }  //Hette ParkingSpotNumber innan och visade vilken plats fordonet är parkerat på
    }
}
