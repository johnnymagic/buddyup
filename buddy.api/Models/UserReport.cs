using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuddyUp.API.Models.Domain
{
    public class UserReport
    {
        [Key]
        public Guid ReportId { get; set; }
        
        [Required]
        public Guid ReportingUserId { get; set; }
        
        [Required]
        public Guid ReportedUserId { get; set; }
        
        [Required]
        public string Reason { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Reviewed, Dismissed, Action Taken
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? ReviewedAt { get; set; }
        
        public Guid? ReviewedByUserId { get; set; }
        
        // Navigation properties
        [ForeignKey("ReportingUserId")]
        public virtual User ReportingUser { get; set; }
        
        [ForeignKey("ReportedUserId")]
        public virtual User ReportedUser { get; set; }
        
        [ForeignKey("ReviewedByUserId")]
        public virtual User ReviewedBy { get; set; }
    }
}