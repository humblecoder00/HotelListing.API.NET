namespace HotelListing.API.Data
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string ShortName { get; set; }

        // Relationship with the Hotel entity:
        // A country can have many hotels, but a hotel can only have one country.
        // Here we represent the one-to-many relationship between the Country and Hotel entities.
        public virtual IList<Hotel>? Hotels { get; set; }

    }
}