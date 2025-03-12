using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuddyUp.API.Models.Domain
{
    public class UserSport
    {
        [Key]
        public Guid UserSportId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        public Guid SportId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string SkillLevel { get; set; } // Beginner, Intermediate, Advanced, Expert
        
        public int? YearsExperience { get; set; }
        
        public string Notes { get; set; }
        
        public bool IsPublic { get; set; } = true;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [ForeignKey("SportId")]
        public virtual Sport Sport { get; set; }
    }
}