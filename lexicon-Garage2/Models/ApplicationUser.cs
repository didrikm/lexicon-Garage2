using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace lexicon_Garage2.Models
{
    public class ApplicationUser : IdentityUser
    {
        [RegularExpression(
            @"^\d{6}-\d{4}$",
            ErrorMessage = "The SSN must be in the format YYYYMMDD-XXXX."
        )]
        public string Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }
}
