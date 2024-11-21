namespace lexicon_Garage2.Models
{
    public class ParkingSpot
    {
        public int Id { get; set; } // Unik identifierare
        public bool IsOccupied { get; set; } // Status: ledig/upptagen

        public Vehicle? Vehicle { get; set; } // Navigeringsproperty till fordon
    }
}
