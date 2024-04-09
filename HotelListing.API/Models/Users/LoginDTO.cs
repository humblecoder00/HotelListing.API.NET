using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Models.Users
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required] // IdentityCore provides default password type check, if pass is too short
        // But we can still let users know our choices:
        [StringLength(15, ErrorMessage = "Your Password is limited to {2} to {1} characters", MinimumLength = 6)]
        public string Password { get; set; }
    }
}
