using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuddyUp.API.Models.Domain
{
    public class Match
    {
        [Key]
        public Guid MatchId { get; set; }
        
        [Required]
        public Guid RequesterId { get; set; }
        
        [Required]
        public Guid RecipientId { get; set; }
        
        public Guid? SportId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Canceled
        
        public DateTime RequestedAt { get; set; }
        
        public DateTime? RespondedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("RequesterId")]
        public virtual User Requester { get; set; }
        
        [ForeignKey("RecipientId")]
        public virtual User Recipient { get; set; }
        
        [ForeignKey("SportId")]
        public virtual Sport Sport { get; set; }
    }
}