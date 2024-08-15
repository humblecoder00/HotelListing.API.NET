using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Core.Models.Hotel
{
    public class CreateHotelDTO : BaseHotelDTO
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int CountryId { get; set; }
    }
}
