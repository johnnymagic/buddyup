using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuddyUp.API.Models.Domain
{
    public class Sport
    {
        [Key]
        public Guid SportId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [MaxLength(255)]
        public string IconUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; }
        
        public Guid? CreatedByUserId { get; set; }
        
        // Navigation properties
        public virtual ICollection<UserSport> UserSports { get; set; }
        
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; }
    }
}