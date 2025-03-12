using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Models.Responses;
using BuddyUp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuddyUp.API.Controllers
{
    [ApiController]
    [Route("api/messaging")]
    [Authorize]
    public class MessagingController : ControllerBase
    {
        private readonly IMessagingService _messagingService;
        private readonly ILogger<MessagingController> _logger;

        public MessagingController(IMessagingService messagingService, ILogger<MessagingController> logger)
        {
            _messagingService = messagingService ?? throw new ArgumentNullException(nameof(messagingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all conversations for the current user
        /// </summary>
        [HttpGet("conversations")]
        public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var conversations = await _messagingService.GetConversations(userId);
                return Ok(conversations);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found during conversations request");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while fetching conversations" });
            }
        }

        /// <summary>
        /// Get a specific conversation by ID
        /// </summary>
        [HttpGet("conversations/{conversationId}")]
        public async Task<ActionResult<ConversationDto>> GetConversation(Guid conversationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var conversation = await _messagingService.GetConversation(conversationId, userId);
                return Ok(conversation);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found error during conversation fetch");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to conversation");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while fetching conversation" });
            }
        }

        /// <summary>
        /// Get messages for a specific conversation
        /// </summary>
        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<ActionResult<MessageListDto>> GetMessages(Guid conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var messages = await _messagingService.GetMessages(conversationId, userId, page, pageSize);
                return Ok(messages);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found error during messages fetch");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to conversation messages");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while fetching messages" });
            }
        }

        /// <summary>
        /// Send a message in a conversation
        /// </summary>
        [HttpPost("conversations/{conversationId}/messages")]
        public async Task<ActionResult<MessageDto>> SendMessage(Guid conversationId, [FromBody] SendMessageDto messageDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var message = await _messagingService.SendMessage(conversationId, userId, messageDto);
                return Created($"api/messaging/conversations/{conversationId}/messages", message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found error during message send");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access when sending message");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when sending message");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while sending message" });
            }
        }

        /// <summary>
        /// Mark all messages in a conversation as read
        /// </summary>
        [HttpPut("conversations/{conversationId}/read")]
        public async Task<ActionResult> MarkMessagesAsRead(Guid conversationId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                await _messagingService.MarkMessagesAsRead(conversationId, userId);
                return Ok(new ApiResponse { Success = true, Message = "Messages marked as read" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found error when marking messages as read");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access when marking messages as read");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while marking messages as read" });
            }
        }

        /// <summary>
        /// Create a new conversation
        /// </summary>
        [HttpPost("conversations")]
        public async Task<ActionResult<ConversationDto>> CreateConversation([FromBody] CreateConversationDto conversationDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var conversation = await _messagingService.CreateConversation(userId, conversationDto);
                return Created($"api/messaging/conversations/{conversation.ConversationId}", conversation);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found error during conversation creation");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during conversation creation");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during conversation creation");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while creating conversation" });
            }
        }

        /// <summary>
        /// Get a conversation with a specific user
        /// </summary>
        [HttpGet("user/{otherUserId}")]
        public async Task<ActionResult<ConversationDto>> GetConversationWithUser(string otherUserId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var conversation = await _messagingService.GetConversationWithUser(userId, otherUserId);
                return Ok(conversation);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found error during user conversation fetch");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during user conversation fetch");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation with user");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while fetching conversation" });
            }
        }

        /// <summary>
        /// Get a conversation for a specific match
        /// </summary>
        [HttpGet("match/{matchId}")]
        public async Task<ActionResult<ConversationDto>> GetConversationForMatch(Guid matchId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var conversation = await _messagingService.GetConversationForMatch(matchId, userId);
                return Ok(conversation);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found error during match conversation fetch");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during match conversation fetch");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during match conversation fetch");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation for match");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while fetching conversation" });
            }
        }

        /// <summary>
        /// Get unread message count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadMessageCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var count = await _messagingService.GetUnreadMessageCount(userId);
                return Ok(count);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found during unread count request");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread message count");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while fetching unread message count" });
            }
        }
    }
}