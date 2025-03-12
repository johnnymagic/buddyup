using System;
using System.ComponentModel.DataAnnotations;

namespace BuddyUp.API.Models.DTOs
{
    public class SportDto
    {
        public Guid SportId { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string IconUrl { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; }
    }
    
    public class SportCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string IconUrl { get; set; }
    }
    
    public class SportUpdateDto
    {
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string IconUrl { get; set; }
        
        public bool? IsActive { get; set; }
    }
}