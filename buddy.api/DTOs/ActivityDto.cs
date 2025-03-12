using System;
using System.ComponentModel.DataAnnotations;

namespace BuddyUp.API.Models.DTOs
{
    public class ActivityDto
    {
        public Guid ActivityId { get; set; }
        
        public Guid SportId { get; set; }
        
        public SportDto Sport { get; set; }
        
        public Guid? LocationId { get; set; }
        
        public LocationDto Location { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string RecurringSchedule { get; set; }
        
        public string DifficultyLevel { get; set; }
        
        public int? MaxParticipants { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; }
        
        public double? Distance { get; set; }
    }
    
    public class ActivityCreateDto
    {
        [Required]
        public Guid SportId { get; set; }
        
        public Guid? LocationId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string RecurringSchedule { get; set; }
        
        public string DifficultyLevel { get; set; }
        
        public int? MaxParticipants { get; set; }
    }
    
    public class ActivityUpdateDto
    {
        public Guid? SportId { get; set; }
        
        public Guid? LocationId { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string RecurringSchedule { get; set; }
        
        public string DifficultyLevel { get; set; }
        
        public int? MaxParticipants { get; set; }
        
        public bool? IsActive { get; set; }
    }
    
    public class ActivitySearchDto
    {
        public Guid? SportId { get; set; }
        
        public double? Latitude { get; set; }
        
        public double? Longitude { get; set; }
        
        public double? Distance { get; set; } // km
        
        public string Difficulty { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
    }
}