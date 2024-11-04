namespace lexicon_Garage2.Models
{
    public class StatisticsViewModel
    {
        public int NumberOfVehiclesParked { get; set; }
        public int NumberOfWheelsInGarage { get; set; }
        public decimal UnrealizedParkingRevenue { get; set; }
        public decimal AccumulatedParkingRevenue { get; set; }
        public StatisticsViewModel() { 
            
        }
    }
}
