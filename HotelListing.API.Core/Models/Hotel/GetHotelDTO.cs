using HotelListing.API.Core.Models.Country;

namespace HotelListing.API.Core.Models.Hotel
{
    public class GetHotelDTO : BaseHotelDTO
    {
        public int Id { get; set; }

        public GetCountryDTO Country { get; set; }

    }
}
