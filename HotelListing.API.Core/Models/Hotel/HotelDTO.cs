using HotelListing.API.Core.Models.Country;

namespace HotelListing.API.Core.Models.Hotel
{
    public class HotelDTO : BaseHotelDTO
    {
            public int Id { get; set; }

            public int CountryId { get; set; }

    }
}
