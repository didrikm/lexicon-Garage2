namespace lexicon_Garage2.Models
{
    public class ReceiptViewModel
    {
        public string RegistrationNumber { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        public float DurationOfParking { get; set; }
        public float Total { get; set; }
        public ReceiptViewModel(Vehicle vehicle)
        {
            RegistrationNumber = vehicle.RegistrationNumber;
            CheckInTime = vehicle.ParkingTime;
            CheckOutTime = DateTime.Now;
            DurationOfParking = (float)(CheckOutTime - CheckInTime).TotalHours;
            Total = 100 * DurationOfParking;
        }
    }
}
