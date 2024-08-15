namespace HotelListing.API.Core.Models.Country
{
    // Note: Ideally you want to share the similar Required attributes between CREATE and UPDATE DTOs
    // to keep the consistency of the data
    public class UpdateCountryDTO : BaseCountryDTO
    {
        public int Id { get; set; }
    }
}
