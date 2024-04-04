using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Data;
using AutoMapper;
using HotelListing.API.Models.Hotel;
using HotelListing.API.Contracts;

namespace HotelListing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly HotelListingDbContext _context;
        private readonly ICountriesRepository _countriesRepository;

        public HotelsController(IMapper mapper, HotelListingDbContext context, ICountriesRepository countriesRepository)
        {
            this._mapper = mapper;
            this._countriesRepository = countriesRepository;
            _context = context;
        }

        // GET: api/Hotels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HotelDTO>>> GetHotels()
        {
            if (_context.Hotels == null)
            {
                return NotFound();
            }

            var hotels = await _context.Hotels.ToListAsync();
            var records = _mapper.Map<List<HotelDTO>>(hotels);

            return Ok(records);
        }

        // GET: api/Hotels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetHotelDTO>> GetHotel(int id)
        {
            /*
             The primary catch here is understanding how to use .Include() to eagerly load related entities. 
            Eager loading is essential when you need related data to be included with your query results 
            to avoid the N+1 query problem and to shape your data as required by the client/consumer of your API.

            The N+1 query problem occurs when an application makes one query to fetch a set of records (N) and 
            then iterates over these records to make an additional query for each one to retrieve related data. 
            This results in 1 initial query plus N additional queries, leading to inefficient database access and poor performance. 
            It's commonly encountered in applications using ORM (Object-Relational Mapping) tools when related entities are not properly pre-loaded.
             */

            var hotel = await _context.Hotels
                .Include(h => h.Country) // Eagerly load the Country related to this Hotel
                .FirstOrDefaultAsync(h => h.Id == id); // Find the hotel by its Id

            if (hotel == null)
            {
                return NotFound();
            }

            var record = _mapper.Map<GetHotelDTO>(hotel); // Map the hotel entity to GetHotelDTO

            return Ok(record);
        }

        // PUT: api/Hotels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHotel(int id, UpdateHotelDTO hotelPayload)
        {
            if (id != hotelPayload.Id)
            {
                return BadRequest();
            }

            // Check first if the country exists

            var countryExists = await _context.Countries.AsNoTracking().AnyAsync(c => c.Id == hotelPayload.CountryId);

            if (!countryExists)
            {
                return BadRequest("Country does not exist.");
            }


            // Check if the hotel exists

            var hotel = await _context.Hotels.FindAsync(id);

            if (hotel == null)
            {
                return NotFound();
            }

            // Entity Framework will automatically track the changes made to the hotel object
            // without having to explicitly set the state to "EntityState.Modified"
            _mapper.Map(hotelPayload, hotel);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HotelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Hotels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Hotel>> PostHotel(CreateHotelDTO hotelPayload)
        {
            if (_context.Hotels == null)
            {
                return Problem("Entity set 'HotelListingDbContext.Hotels'  is null.");
            }

            var countryExists = await _context.Countries.AsNoTracking().AnyAsync(c => c.Id == hotelPayload.CountryId);

            /*
             // Note: About the "System.Text.Json.JsonException: A possible object cycle was detected...":

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

            if (!countryExists)
            {
                return BadRequest("Country does not exist.");
            }

            var newHotel = _mapper.Map<Hotel>(hotelPayload);

            await _context.AddAsync(newHotel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHotel", new { id = newHotel.Id }, newHotel);
        }

        // DELETE: api/Hotels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            if (_context.Hotels == null)
            {
                return NotFound();
            }
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HotelExists(int id)
        {
            return (_context.Hotels?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
