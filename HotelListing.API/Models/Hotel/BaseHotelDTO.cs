using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Models.Hotel
{
    // abstract classes can't be initiated, but can be used for inheritance purposes
    // you can place common properties that is used across multiple DTOs in this class
    public abstract class BaseHotelDTO
    {
        // Note: When getting the data, [Required] attribute is not checked
        // but when posting data, [Required] attribute is checked,
        // so it can be a shared property for both GET and POST requests
        [Required]
        public string Name { get; set; }
        public string Address { get; set; }
        public double Rating { get; set; }
    }
}
