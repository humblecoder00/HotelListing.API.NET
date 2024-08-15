// Includes the Entity Framework Core namespace, which provides the DbContext and other EF Core functionalities
using HotelListing.API.Data.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

// Declares the namespace for your DbContext class. Namespaces organize your code and prevent naming conflicts.
namespace HotelListing.API.Data
{
    // Defines your DbContext class, HotelListingDbContext, which inherits from the EF Core DbContext class.
    // DbContext is a key class in EF Core that manages database connections and caching.
    // It is a contract between the application and the database, and is used to query and save data.
    // We have to let it know about the database tables we have.

    // NOTE: When adding the Identity Core, you should inherit from IdentityDbContext<ApiUser> instead of DbContext:
    // This is because the Identity Core requires a DbContext that is aware of the user type.
    public class HotelListingDbContext : IdentityDbContext<ApiUser>
    {
        // Constructor for your DbContext. It accepts DbContextOptions, which is used to configure the DbContext.
        // The ": base(options)" part calls the base class constructor with the options, enabling EF Core configurations.
        public HotelListingDbContext(DbContextOptions options) : base(options)
        {

        }

        // Inside the DbContext class, you would typically include properties of type DbSet<T> for each entity to be included in the model.
        // Example: public DbSet<Hotel> Hotels { get; set; }
        // This allows you to query and save instances of these entities by using LINQ directly on these properties.

        // ---------------------------------------------------------------------------------------------------------------------

        // Add tables to the DB:
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Country> Countries { get; set; }


        // The OnModelCreating method is called by the framework when the EF Core model is being created.
        // This method is a place to configure entity behaviors, relationships, and to seed initial data.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // This calls the base implementation of OnModelCreating.
            // It's important to call this when overriding OnModelCreating to ensure that
            // any configuration done in base classes is also applied.
            // For example, if you're using ASP.NET Identity, it configures the identity model.
            base.OnModelCreating(modelBuilder);

            // APPLY CONFIGURATIONS:

            modelBuilder.ApplyConfiguration(new RoleConfiguration()); // Applies the RoleConfiguration class to the model.

            // This configures the 'Country' entity. It specifies that when the database is created,
            // the 'Countries' table should be seeded with the specified initial data.

            // NOTE: Here it makes sense to seed the Country data first.
            // This is because the Hotel needs a Country to be associated with.
            modelBuilder.ApplyConfiguration(new CountryConfiguration()); // Applies the CountryConfiguration class to the model.


            // Similar to the 'Country' entity, this configures the 'Hotel' entity.
            // It seeds the 'Hotels' table with the specified initial data.
            modelBuilder.ApplyConfiguration(new HotelConfiguration()); // Applies the HotelConfiguration class to the model.
        }
    }

    public class HotelListingDbContextFactory : IDesignTimeDbContextFactory<HotelListingDbContext>
    {
        public HotelListingDbContext CreateDbContext(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<HotelListingDbContext>();
            var conn = config.GetConnectionString("HotelListingDbConnectionString");
            optionsBuilder.UseSqlServer(conn);
            return new HotelListingDbContext(optionsBuilder.Options);

        }
    }
}