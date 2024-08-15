namespace HotelListing.API.Core.Models.Hotel
{
    // Note: Ideally you want to share the similar Required attributes between CREATE and UPDATE DTOs
    // to keep the consistency of the data
    public class UpdateHotelDTO : BaseHotelDTO
    {
        public int Id { get; set; }

        public int CountryId { get; set; }
    }
}
