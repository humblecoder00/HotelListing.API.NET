using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.API.Data
{
    // This class represents an entity model, which is used to model the data in the database.
    public class Hotel
    {
        // primary key, Entity Framework will recognize this as the primary key
        // and will auto-increment it. It can also recognize if we name it as HotelId (Hotel (entity name) + Id)
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public double Rating { get; set; }

        // ignore writing ForeignKey name via "magic string" like this: [ForeignKey("CountryId")]
        // instead, use nameof() to avoid typos and refactoring issues.
        // nameof() turns the parameter name into a string during runtime:
        [ForeignKey(nameof(CountryId))]
        public int CountryId { get; set; } // foreign key

        // Relationship with the Country entity:
        // A country can have many hotels, but a hotel can only have one country.
        // Here we represent the one-to-many relationship between the Country and Hotel entities.
        public Country Country { get; set; }
    }
}
