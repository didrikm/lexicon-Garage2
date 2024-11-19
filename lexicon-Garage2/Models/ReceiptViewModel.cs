namespace lexicon_Garage2.Models
{
    public class ReceiptViewModel
    {
        public string RegistrationNumber { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        public TimeSpan DurationOfParking { get; set; }
        public decimal Total { get; set; }

        public ReceiptViewModel(Vehicle vehicle, decimal parkingHourlyPrice)
        {
            RegistrationNumber = vehicle.RegistrationNumber;
            CheckInTime = vehicle.ParkingTime;
            CheckOutTime = DateTime.Now;
            DurationOfParking = CheckOutTime - CheckInTime;
            Total = parkingHourlyPrice * (decimal)DurationOfParking.TotalHours;
        }

        public string GetFormattedDuration()
        {
            return $"{DurationOfParking.Hours} h {DurationOfParking.Minutes} m {DurationOfParking.Seconds} s";
        }
    }
}
