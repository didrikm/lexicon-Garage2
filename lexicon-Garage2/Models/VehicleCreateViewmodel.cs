using System.ComponentModel.DataAnnotations;

namespace lexicon_Garage2.Models
{
    public class VehicleCreateViewmodel
    {

        public int VehicleTypeId { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string RegistrationNumber { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Color { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Brand { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Model { get; set; }

        [Required]
        [Range(0, 20)]
        public int NumberOfWheels { get; set; }

    }
}
