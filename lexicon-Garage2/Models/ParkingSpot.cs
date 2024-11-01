namespace lexicon_Garage2.Models
{
    public class ParkingSpot
    {
        public int Id { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int? VehicleId { get; set; }
    }
}
