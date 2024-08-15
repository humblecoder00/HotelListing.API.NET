using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Data
{
    // IdentityUser is a class that represents a user in the identity system.
    public class ApiUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // extend the user with additional properties if needed
    }
}
