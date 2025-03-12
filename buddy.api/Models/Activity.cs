using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuddyUp.API.Models.Domain
{
    public class Activity
    {
        [Key]
        public Guid ActivityId { get; set; }
        
        [Required]
        public Guid SportId { get; set; }
        
        public Guid? LocationId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        // JSON format for recurring patterns
        [MaxLength(255)]
        public string RecurringSchedule { get; set; }
        
        [MaxLength(50)]
        public string DifficultyLevel { get; set; }
        
        public int? MaxParticipants { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; }
        
        public Guid? CreatedByUserId { get; set; }
        
        // Navigation properties
        [ForeignKey("SportId")]
        public virtual Sport Sport { get; set; }
        
        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }
        
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; }
    }
}