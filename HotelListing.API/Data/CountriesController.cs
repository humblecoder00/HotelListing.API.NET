using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Data
{
    // Attributes that define the route for the API controller and specify that this controller responds to web API requests.
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        // HERE, WE'RE INJECTING OUR DB CONTEXT INTO THE CONTROLLER
        // This DB context is singular, we don't have to declare a new instance of the DB context.
        // We can simply inject it. This relates to the "I" of the SOLID principles, the Inversion of Control (IoC).

        /*
        Inversion of Control (IoC) is a principle where the control over parts of a program is transferred from the programmer to a framework or container.

        Simple Example: Instead of a chef deciding on the menu (traditional approach), a meal kit service provides the ingredients and recipes, 
        and the chef follows those instructions (IoC). The service dictates the "what" and "how," not the chef.
         */
        private readonly HotelListingDbContext _context;

        public CountriesController(HotelListingDbContext context)
        {
            _context = context;
        }

        // GET: api/Countries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
          if (_context.Countries == null)
          {
              return NotFound();
          }
            var countries = await _context.Countries.ToListAsync();
            return Ok(countries);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountry(int id)
        {
          if (_context.Countries == null)
          {
              return NotFound();
          }
            var country = await _context.Countries.FindAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            return Ok(country);
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, Country country)
        {
            if (id != country.Id)
            {
                return BadRequest("Invalid record id");
            }

            // Every entity in EF has something called "Entity State".
            // This is a property that tells EF what to do with the entity.
            // So when we save changes below, EF knows that it should just be an update.
            _context.Entry(country).State = EntityState.Modified;

            // We have try catch, because maybe two separate requests are trying to update the same record at the same time.
            // DbUpdateConcurrencyException is thrown when a database operation fails because another concurrent operation has updated the same data.
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // This is actually a 204 response, says successful but does not return anything.
            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(Country country)
        {
          if (_context.Countries == null)
          {
              return Problem("Entity set 'HotelListingDbContext.Countries'  is null.");
          }
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            if (_context.Countries == null)
            {
                return NotFound();
            }
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CountryExists(int id)
        {
            return (_context.Countries?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
