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
        private readonly IHotelsRepository _hotelsRepository;
        private readonly ICountriesRepository _countriesRepository;

        public HotelsController(IMapper mapper, IHotelsRepository hotelsRepository, ICountriesRepository countriesRepository)
        {
            this._mapper = mapper;
            this._hotelsRepository = hotelsRepository;
            this._countriesRepository = countriesRepository;
        }

        // GET: api/Hotels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HotelDTO>>> GetHotels()
        {
            var hotels = await _hotelsRepository.GetAllAsync();
            var records = _mapper.Map<List<HotelDTO>>(hotels);

            return Ok(records);
        }

        // GET: api/Hotels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetHotelDTO>> GetHotel(int id)
        {
            var hotel = await _hotelsRepository.GetDetails(id);

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

            // Check if the country exists
            var country = await _countriesRepository.CountryExists(hotelPayload.CountryId);

            if (country == null)
            {
                return BadRequest("Country does not exist.");
            }

            // Check if the hotel exists
            var hotel = await _hotelsRepository.GetAsync(id);

            if (hotel == null)
            {
                return NotFound();
            }

            // Entity Framework will automatically track the changes made to the hotel object
            // without having to explicitly set the state to "EntityState.Modified"
            // hotelPayload -> maps to -> hotel
            _mapper.Map(hotelPayload, hotel);

            try
            {
                await _hotelsRepository.UpdateAsync(hotel);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await HotelExists(id))
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
            // Check if the country exists
            var country = await _countriesRepository.CountryExists(hotelPayload.CountryId);

            if (!country)
            {
                return BadRequest("Country does not exist.");
            }

            var newHotel = _mapper.Map<Hotel>(hotelPayload);

            await _hotelsRepository.AddAsync(newHotel);

            return CreatedAtAction("GetHotel", new { id = newHotel.Id }, newHotel);
        }

        // DELETE: api/Hotels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _hotelsRepository.GetAsync(id);

            if (hotel == null)
            {
                return NotFound();
            }

            await _hotelsRepository.DeleteAsync(id);

            return NoContent();
        }

        private async Task<bool> HotelExists(int id)
        {
            return await _hotelsRepository.Exists(id);
        }
    }
}
