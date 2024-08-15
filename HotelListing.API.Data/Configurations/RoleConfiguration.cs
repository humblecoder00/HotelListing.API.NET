using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.API.Data.Configurations
{
    // Here we're using this class to seed the roles into the database, as a configuration
    // As a matter of fact, you can have Configuration classes for any sort of operations you need to do
    // in the database, like changing table names, etc.
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole
                {
                    Name = "Administrator",
                    NormalizedName = "ADMINISTRATOR",
                },
                new IdentityRole
                {
                    Name = "User",
                    NormalizedName = "USER"
                }
            );
        }
    }
}
