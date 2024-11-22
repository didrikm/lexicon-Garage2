﻿namespace lexicon_Garage2.Models.ViewModels
{
    public class VehicleViewModel
    {
        public VehicleViewModel() { }

        public VehicleViewModel(Vehicle vehicle)
        {
            Id = vehicle.Id;
            VehicleType = vehicle.VehicleType;
            RegistrationNumber = vehicle.RegistrationNumber;
            ArrivalTime = vehicle.ParkingTime;
            //ParkingSpotNumber = vehicle.ParkingSpots;
            Color = vehicle.Color;
            Brand = vehicle.Brand;
            Model = vehicle.Model;
            NumberOfWheels = vehicle.NumberOfWheels;
            ParkingSpotsTaken = vehicle.ParkingSpots.Count;
        }

        public int Id { get; set; }
        public VehicleType VehicleType { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int ParkingSpot { get; set; }
        public int? ParkingSpotNumber { get; set; }

        // New Properties
        public string Color { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int NumberOfWheels { get; set; }

        //public int ParkingSpot { get; set; }
        public int? ParkingSpotsTaken { get; set; } //Hette ParkingSpotNumber innan och visade vilken plats fordonet är parkerat på
    }
}