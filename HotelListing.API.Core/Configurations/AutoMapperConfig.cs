using AutoMapper;
using HotelListing.API.Core.Models.Country;
using HotelListing.API.Core.Models.Hotel;
using HotelListing.API.Core.Models.Users;
using HotelListing.API.Data;

namespace HotelListing.API.Core.Configurations
{
    public class AutoMapperConfig : Profile
    {
        // This constructor allows us to create mappings between the source and destination data types.
        public AutoMapperConfig()
        {
            // ReverseMap() allows us to map the destination data type back to the source data type: Country <-> CreateCountryDTO
            CreateMap<Country, CreateCountryDTO>().ReverseMap();
            CreateMap<Country, GetCountryDTO>().ReverseMap();
            CreateMap<Country, CountryDTO>().ReverseMap();
            CreateMap<Country, UpdateCountryDTO>().ReverseMap();

            CreateMap<Hotel, HotelDTO>().ReverseMap();
            CreateMap<Hotel, GetHotelDTO>().ReverseMap();
            CreateMap<Hotel, CreateHotelDTO>().ReverseMap();
            CreateMap<Hotel, UpdateHotelDTO>().ReverseMap();

            CreateMap<ApiUser, ApiUserDTO>().ReverseMap();
        }
    }
}
