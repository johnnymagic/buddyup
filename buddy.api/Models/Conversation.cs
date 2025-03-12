using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuddyUp.API.Models.Domain
{
    public class Conversation
    {
        [Key]
        public Guid ConversationId { get; set; }
        
        public Guid? MatchId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastMessageAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        [ForeignKey("MatchId")]
        public virtual Match Match { get; set; }
        
        public virtual ICollection<Message> Messages { get; set; }
    }
}
