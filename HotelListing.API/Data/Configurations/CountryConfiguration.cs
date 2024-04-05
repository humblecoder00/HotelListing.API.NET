using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.API.Data.Configurations
{
    // Here we're using this class to seed the countries into the database, as a configuration
    // As a matter of fact, you can have Configuration classes for any sort of operations you need to do
    // in the database, like changing table names, etc.
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.HasData(
                new Country
                {
                    Id = 1, // Specifies the primary key value of the 'Country' entity.
                    Name = "Jamaica", // Sets the 'Name' property for this 'Country' entity.
                    ShortName = "JM" // Sets the 'ShortName' property for this 'Country' entity.
                },
                new Country
                {
                    Id = 2, // Same as above, for a different 'Country' entity.
                    Name = "Bahamas",
                    ShortName = "BS"
                },
                new Country
                {
                    Id = 3, // And again, for another 'Country' entity.
                    Name = "Cayman Island",
                    ShortName = "CI"
                }
            );
        }
    }
}
