namespace lexicon_Garage2.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string VehicleType { get; set; }
        public string RegistrationNumber { get; set; }
        public string Color { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int NumberOfWheels { get; set; }
        public DateTime ParkingTime { get; set; }
        public Vehicle()
        {
            ParkingTime = DateTime.Now;
        }
    }
}
