using lexicon_Garage2.Models.ViewModels;

namespace lexicon_Garage2.Models
{
    public class MemberViewModel
    {
        public string Owner { get; set; } // Namn på medlem
        public int VehicleCount { get; set; } // Antal fordon
        public decimal TotalPrice { get; set; } // Totalkostnad för alla fordon
        public List<VehicleViewModel> Vehicles { get; set; } // Lista över fordon

        public MemberViewModel(ApplicationUser user, decimal parkingHourlyPrice)
        {
            Owner = $"{user.FirstName} {user.LastName}";
            VehicleCount = user.Vehicles.Count;

            // Totalpris för alla fordon
            TotalPrice = user.Vehicles.Sum(vehicle =>
                parkingHourlyPrice * (decimal)(DateTime.Now - vehicle.ParkingTime).TotalHours
            );

            // Skapar en lista av VehicleViewModel
            Vehicles = user.Vehicles.Select(v => new VehicleViewModel(v)).ToList();
        }
    }
}
