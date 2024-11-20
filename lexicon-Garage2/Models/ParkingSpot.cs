namespace lexicon_Garage2.Models
{
    public class ParkingSpot
    {
        public int Id { get; set; } // Unik identifierare
        public int SpotNumber { get; set; } // Parkeringsplatsens nummer
        public bool IsOccupied { get; set; } // Status: ledig/upptagen
        public string? RegistrationNumber { get; set; } // Fordonets registreringsnummer

        public int? VehicleId { get; set; } // Kopplar till Vehicle
        public Vehicle? Vehicle { get; set; } // Navigeringsproperty till fordon
        public int MaxCapacity { get; set; } = 10;
    }
}
