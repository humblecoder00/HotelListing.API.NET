using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Repository;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Repository
{
    public class HotelsRepository : GenericRepository<Hotel>, IHotelsRepository
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

        public HotelsRepository(HotelListingDbContext context, IMapper mapper) : base(context, mapper)
        {
            this._context = context;
        }

        public async Task<Hotel> GetDetails(int id)
        {
            // Include() is an inner join in EF Core.
            // We're saying, Go to the Countries table -> Include the list of Hotels,
            // and then find the first record where the Id matches the Id that was passed in.

            /*
            The primary catch here is understanding how to use .Include() to eagerly load related entities. 
            Eager loading is essential when you need related data to be included with your query results 
            to avoid the N+1 query problem and to shape your data as required by the client/consumer of your API.

            The N+1 query problem occurs when an application makes one query to fetch a set of records (N) and 
            then iterates over these records to make an additional query for each one to retrieve related data. 
            This results in 1 initial query plus N additional queries, leading to inefficient database access and poor performance. 
            It's commonly encountered in applications using ORM (Object-Relational Mapping) tools when related entities are not properly pre-loaded.
            */

            return await _context.Hotels
                .Include(h => h.Country) // Eagerly load the Country related to this Hotel
                .FirstOrDefaultAsync(h => h.Id == id); // Find the hotel by its Id
        }
    }
}
