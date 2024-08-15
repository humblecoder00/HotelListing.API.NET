using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using HotelListing.API.Core.Models.Country;
using AutoMapper;
using HotelListing.API.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using HotelListing.API.Core.Exceptions;
using Asp.Versioning;
using HotelListing.API.Core.Models;

namespace HotelListing.API.Data
{
    // Attributes that define the route for the API controller and specify that this controller responds to web API requests.
    [Route("api/v{version:apiVersion}/countries")]
    [ApiController]
    [ApiVersion("1.0", Deprecated = true)]
    public class CountriesController : ControllerBase
    {
        // IMapper is a service that can map objects of one type to another.
        private readonly IMapper _mapper;
        // Repository handles the DB operations in detail, so it is abstracted from the controller.
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(IMapper mapper, ICountriesRepository countriesRepository, ILogger<CountriesController> logger)
        {
            this._mapper = mapper;
            this._countriesRepository = countriesRepository;
            this._logger = logger;
        }

        // GET: api/Countries/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<GetCountryDTO>>> GetCountries()
        {
            // Mapping DTOs & error handling is done at the generic repository.
            var countries = await _countriesRepository.GetAllAsync<GetCountryDTO>();
            return Ok(countries);
        }

        // GET: api/Countries/?StartIndex=0&PageSize=25PageNumber=1
        [HttpGet]
        public async Task<ActionResult<PagedResult<GetCountryDTO>>> GetPagedCountries([FromQuery] QueryParameters queryParameters)
        {
            var pagedCountriesResult = await _countriesRepository.GetAllAsync<GetCountryDTO>(queryParameters);
            return Ok(pagedCountriesResult);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDTO>> GetCountry(int id)
        {
            // Mapping DTOs & error handling is done at the generic repository.
            var country = await _countriesRepository.GetDetails(id);
            return Ok(country);
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDTO updateCountryDto)
        {
            if (id != updateCountryDto.Id)
            {
                return BadRequest("Invalid record id");
            }

            // Entity Framework will automatically track the changes made to the country object
            // without having to explicitly set the state to "EntityState.Modified"
            //_mapper.Map(countryPayload, country);

            // We have try catch, because maybe two separate requests are trying to update the same record at the same time.
            // DbUpdateConcurrencyException is thrown when a database operation fails because another concurrent operation has updated the same data.
            try
            {
                await _countriesRepository.UpdateAsync(id, updateCountryDto);
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
        public async Task<ActionResult<CountryDTO>> PostCountry(CreateCountryDTO countryPayload)
        {
            var country = await _countriesRepository.AddAsync<CreateCountryDTO, GetCountryDTO>(countryPayload);
            return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize(Roles ="Administrator")] // can be extended for multiple roles like: "Administrator,User,..."
        public async Task<IActionResult> DeleteCountry(int id)
        {
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