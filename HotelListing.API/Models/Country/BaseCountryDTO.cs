﻿using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Models.Country
{
    // abstract classes can't be initiated, but can be used for inheritance purposes
    // you can place common properties that is used across multiple DTOs in this class
    public abstract class BaseCountryDTO
    {
        // Note: When getting the data, [Required] attribute is not checked
        // but when posting data, [Required] attribute is checked,
        // so it can be a shared property for both GET and POST requests
        [Required]
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
}
