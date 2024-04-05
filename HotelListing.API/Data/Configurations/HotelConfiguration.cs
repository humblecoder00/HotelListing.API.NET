using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.API.Data.Configurations
{
    public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            builder.HasData(
                new Hotel
                {
                    Id = 1, // Specifies the primary key value of the 'Hotel' entity.
                    Name = "Sandals Resort and Spa", // Sets the 'Name' property.
                    Address = "Negril", // Sets the 'Address' property.
                    CountryId = 1, // Foreign key linking this 'Hotel' to its 'Country'.
                    Rating = 4.5 // Sets the 'Rating' property.
                },
                new Hotel
                {
                    Id = 2, // And so on, for each 'Hotel' entity to seed.
                    Name = "Comfort Suites",
                    Address = "George Town",
                    CountryId = 3,
                    Rating = 4.3
                },
                new Hotel
                {
                    Id = 3,
                    Name = "Grand Palldium",
                    Address = "Nassua",
                    CountryId = 2,
                    Rating = 4
                }
            );
        }
    }
}
