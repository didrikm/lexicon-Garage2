namespace lexicon_Garage2.Models
{
    public class StatisticsViewModel
    {
        public int NumberOfVehiclesParked { get; set; }
        public int NumberOfWheelsInGarage { get; set; }
        public decimal UnrealizedParkingRevenue { get; set; }
        public decimal AccumulatedParkingRevenueView { get; set; }
        public int TotalParkingSpots { get; set; } // Totalt antal platser
        public int OccupiedParkingSpots { get; set; } // Upptagna platser
        public int FreeParkingSpots { get; set; } // Lediga platser

        public StatisticsViewModel() { }
    }
}
