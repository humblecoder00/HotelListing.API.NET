using HotelListing.API.Contracts;
using HotelListing.API.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Repository
{
    // CountriesRepository inherits from the GenericRepository class and implements the ICountriesRepository interface.
    public class CountriesRepository : GenericRepository<Country>, ICountriesRepository
    {
        // HERE, WE'RE INJECTING OUR DB CONTEXT INTO THE REPOSITORY
        // This DB context is singular, we don't have to declare a new instance of the DB context.
        // We can simply inject it. This relates to the "I" of the SOLID principles, the Inversion of Control (IoC).

        /*
        Inversion of Control (IoC) is a principle where the control over parts of a program is transferred from the programmer to a framework or container.

        Simple Example: Instead of a chef deciding on the menu (traditional approach), a meal kit service provides the ingredients and recipes, 
        and the chef follows those instructions (IoC). The service dictates the "what" and "how," not the chef.
         */
        private readonly HotelListingDbContext _context;

        public CountriesRepository(HotelListingDbContext context) : base(context)
        {
            this._context = context;
        }

        public async Task<Country> GetDetails(int id)
        {
            // Include() is an inner join in EF Core.
            // We're saying, Go to the Countries table -> Include the list of Hotels,
            // and then find the first record where the Id matches the Id that was passed in:
            return await _context.Countries.Include(x => x.Hotels)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> CountryExists(int id)
        {
            /*
            // Note: Added this method to prevent the error caused by circular references when querying the database. (Exists method usage in GenericRepository)
            
            About the error "System.Text.Json.JsonException: A possible object cycle was detected...":

            Circular reference error occurs when Entity Framework Core's change tracking creates complex object graphs 
            with circular references (e.g., Country -> Hotels -> Country), and System.Text.Json attempts to serialize these for responses. 
            This leads to serialization exceptions due to infinite recursion. To avoid this, consider:
            // 1. Using .AsNoTracking() for read-only queries to prevent unnecessary entity tracking.
            // 2. Managing eager loading to avoid unintentional loading of large object graphs.
            // 3. Utilizing DTOs to structure response data, preventing circular references.
            // 4. Optionally, configure System.Text.Json to handle circular references, though it's preferable to structure data to avoid them.

            Here is an example of problematic querying:
            //var country = await _countriesRepository.Exists(hotelPayload.CountryId); // this causes errors
             */
            return await _context.Countries.AnyAsync(x => x.Id == id);
        }
    }
}
