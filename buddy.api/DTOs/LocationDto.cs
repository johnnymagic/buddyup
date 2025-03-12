using System;
using System.ComponentModel.DataAnnotations;

namespace BuddyUp.API.Models.DTOs
{
    public class LocationDto
    {
        public Guid LocationId { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string Address { get; set; }
        
        public string City { get; set; }
        
        public string State { get; set; }
        
        public string PostalCode { get; set; }
        
        public string Country { get; set; }
        
        public double Latitude { get; set; }
        
        public double Longitude { get; set; }
        
        public bool IsVerified { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; }
    }
    
    public class LocationCreateDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        
        public string Address { get; set; }
        
        [Required]
        public string City { get; set; }
        
        [Required]
        public string State { get; set; }
        
        public string PostalCode { get; set; }
        
        [Required]
        public string Country { get; set; }
        
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }
        
        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }
    }
    
    public class LocationUpdateDto
    {
        public string Name { get; set; }
        
        public string Address { get; set; }
        
        public string City { get; set; }
        
        public string State { get; set; }
        
        public string PostalCode { get; set; }
        
        public string Country { get; set; }
        
        [Range(-90, 90)]
        public double? Latitude { get; set; }
        
        [Range(-180, 180)]
        public double? Longitude { get; set; }
        
        public bool? IsVerified { get; set; }
        
        public bool? IsActive { get; set; }
    }
    
    public class LocationSearchDto
    {
        public string Name { get; set; }
        
        public string City { get; set; }
        
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }
        
        public double? Distance { get; set; } // km
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
    }
}