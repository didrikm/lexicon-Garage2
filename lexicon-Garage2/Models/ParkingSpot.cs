namespace lexicon_Garage2.Models
{
    public class ParkingSpot
    {
        public int Id { get; set; } // Unik identifierare
        public int SpotNumber { get; set; }

        public bool IsOccupied { get; set; } = false; // Status: ledig/upptagen

        public Vehicle? Vehicle { get; set; }
        // Navigeringsproperty till fordon
    }
}
