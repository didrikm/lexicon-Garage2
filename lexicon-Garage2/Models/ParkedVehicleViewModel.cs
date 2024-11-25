namespace lexicon_Garage2.Models
{
    public class ParkedVehicleViewModel
    {
        public string Owner { get; set; } // Owner: "Firstname Lastname"
        public string RegistrationNumber { get; set; }
        public DateTime ArrivalTime { get; set; }
        public VehicleType VehicleType { get; set; }

        public ParkedVehicleViewModel(Vehicle vehicle, ApplicationUser user)
        {
            RegistrationNumber = vehicle.RegistrationNumber;
            ArrivalTime = vehicle.ParkingTime;

            // Kombinera FirstName och LastName till Owner
            // Hantera null-referens för ApplicationUser
            Owner = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown Owner";
        }
    }
}
