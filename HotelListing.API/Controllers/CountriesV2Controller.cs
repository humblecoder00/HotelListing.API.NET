using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using HotelListing.API.Models.Country;
using AutoMapper;
using HotelListing.API.Contracts;
using Microsoft.AspNetCore.Authorization;
using HotelListing.API.Exceptions;
using Asp.Versioning;
using Microsoft.AspNetCore.OData.Query;

namespace HotelListing.API.Data
{
    // Attributes that define the route for the API controller and specify that this controller responds to web API requests.
    [Route("api/v{version:apiVersion}/countries")]
    [ApiController]
    [ApiVersion("2.0")] // NOTE: This will work in the Postman, to make it work in Swagger you need additional configuration.
    public class CountriesV2Controller : ControllerBase
    {
        // IMapper is a service that can map objects of one type to another.
        private readonly IMapper _mapper;
        // Repository handles the DB operations in detail, so it is abstracted from the controller.
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesV2Controller> _logger;

        public CountriesV2Controller(IMapper mapper, ICountriesRepository countriesRepository, ILogger<CountriesV2Controller> logger)
        {
            this._mapper = mapper;
            this._countriesRepository = countriesRepository;
            this._logger = logger;
        }

        // GET: api/Countries
        // Endpoint with OData (EnableQuery) support
        // This can be called like: api/v2/countries?$select=name,shortname&$filter=name eq 'Cuba'&$orderby=name
        // Or, one by one such as: api/v2/countries?$select=name
        [HttpGet]
        [EnableQuery]
        public async Task<ActionResult<IEnumerable<GetCountryDTO>>> GetCountries()
        {
            var countries = await _countriesRepository.GetAllAsync();
            // map them to the correct format
            // considering here we have a list of countries
            var records = _mapper.Map<List<GetCountryDTO>>(countries);
            return Ok(records);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDTO>> GetCountry(int id)
        {
            var country = await _countriesRepository.GetDetails(id);

            if (country == null)
            {
                throw new NotFoundException(nameof(GetCountry), id);
            }

            // single record:
            var record = _mapper.Map<CountryDTO>(country);

            return Ok(record);
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDTO countryPayload)
        {
            if (id != countryPayload.Id)
            {
                return BadRequest("Invalid record id");
            }

            var country = await _countriesRepository.GetAsync(id);

            if (country == null)
            {
                throw new NotFoundException(nameof(GetCountries), id);
            }

            // Entity Framework will automatically track the changes made to the country object
            // without having to explicitly set the state to "EntityState.Modified"
            _mapper.Map(countryPayload, country);

            // We have try catch, because maybe two separate requests are trying to update the same record at the same time.
            // DbUpdateConcurrencyException is thrown when a database operation fails because another concurrent operation has updated the same data.
            try
            {
                await _countriesRepository.UpdateAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CountryExists(id))
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
        [Authorize]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDTO countryPayload)
        {
            var country = _mapper.Map<Country>(countryPayload);

            await _countriesRepository.AddAsync(country);

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize(Roles ="Administrator")] // can be extended for multiple roles like: "Administrator,User,..."
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _countriesRepository.GetAsync(id);

            if (country == null)
            {
                throw new NotFoundException(nameof(GetCountries), id);
            }

            await _countriesRepository.DeleteAsync(id);

            // This is actually a 204 response, says successful but does not return anything.
            return NoContent();
        }

        private async Task<bool> CountryExists(int id)
        {
            return await _countriesRepository.GetAsync(id) != null;
        }
    }
}