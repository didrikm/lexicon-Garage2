﻿namespace lexicon_Garage2.Models
{
    public class ReceiptViewModel
    {
        public ReceiptViewModel() { }

        public int Id { get; set; }
        public VehicleType VehicleType { get; set; }

        public DateTime ArrivalTime { get; set; }
        public int? ParkingSpot { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        public TimeSpan DurationOfParking { get; set; }
        public decimal Total { get; set; }

        public ReceiptViewModel(Vehicle vehicle)
        {
            RegistrationNumber = vehicle.RegistrationNumber;
            CheckInTime = vehicle.ParkingTime;
            CheckOutTime = DateTime.Now;
            DurationOfParking = CheckOutTime - CheckInTime;
            Total = 100 * (decimal)DurationOfParking.TotalHours;
        }

        public string GetFormattedDuration()
        {
            return $"{DurationOfParking.Hours} h {DurationOfParking.Minutes} m {DurationOfParking.Seconds} s";
        }
    }
}
