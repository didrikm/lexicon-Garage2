namespace lexicon_Garage2.Models
{
    public class ParkingSpot
    {
        public int Id { get; set; } // Unik identifierare

        public bool IsOccupied { get; set; } = false; // Status: ledig/upptagen

        public Vehicle? Vehicle
        {
            get { return Vehicle; }
            set
            {
                if (value == null)
                {
                    IsOccupied = false;
                }
                else
                {
                    IsOccupied = true;
                }
                Vehicle = value;
            }
        }
        // Navigeringsproperty till fordon
    }
}
