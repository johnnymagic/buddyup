using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuddyUp.API.Models.Domain
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }
        
        [Required]
        public Guid ConversationId { get; set; }
        
        [Required]
        public Guid SenderId { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public DateTime SentAt { get; set; }
        
        public DateTime? ReadAt { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        
        // Navigation properties
        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; }
        
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }
    }
}