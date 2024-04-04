using HotelListing.API.Models.Country;

namespace HotelListing.API.Models.Hotel
{
    public class GetHotelDTO : BaseHotelDTO
    {
        public int Id { get; set; }

        public GetCountryDTO Country { get; set; }

    }
}
