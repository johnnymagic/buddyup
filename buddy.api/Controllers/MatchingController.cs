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
    [Route("api/matching")]
    [Authorize]
    public class MatchingController : ControllerBase
    {
        private readonly IMatchingService _matchingService;
        private readonly ILogger<MatchingController> _logger;

        public MatchingController(IMatchingService matchingService, ILogger<MatchingController> logger)
        {
            _matchingService = matchingService ?? throw new ArgumentNullException(nameof(matchingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get potential matches based on filter criteria
        /// </summary>
        [HttpGet("potential")]
        public async Task<ActionResult<IEnumerable<PotentialMatchDto>>> GetPotentialMatches([FromQuery] MatchFilterDto filter)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var matches = await _matchingService.GetPotentialMatches(userId, filter);
                return Ok(matches);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found during potential matches request");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting potential matches");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while fetching potential matches" });
            }
        }

        /// <summary>
        /// Get current (accepted) matches
        /// </summary>
        [HttpGet("current")]
        public async Task<ActionResult<IEnumerable<MatchDto>>> GetCurrentMatches()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var matches = await _matchingService.GetCurrentMatches(userId);
                return Ok(matches);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found during current matches request");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current matches");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while fetching current matches" });
            }
        }

        /// <summary>
        /// Get sent match requests
        /// </summary>
        [HttpGet("sent")]
        public async Task<ActionResult<IEnumerable<MatchDto>>> GetSentRequests()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var matches = await _matchingService.GetSentRequests(userId);
                return Ok(matches);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found during sent requests fetch");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sent requests");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while fetching sent requests" });
            }
        }

        /// <summary>
        /// Get received match requests
        /// </summary>
        [HttpGet("received")]
        public async Task<ActionResult<IEnumerable<MatchDto>>> GetReceivedRequests()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var matches = await _matchingService.GetReceivedRequests(userId);
                return Ok(matches);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found during received requests fetch");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting received requests");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while fetching received requests" });
            }
        }

        /// <summary>
        /// Send a match request
        /// </summary>
        [HttpPost("request")]
        public async Task<ActionResult<MatchDto>> SendMatchRequest([FromBody] MatchRequestDto requestDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var match = await _matchingService.SendMatchRequest(userId, requestDto);
                return CreatedAtAction(nameof(GetMatchById), new { matchId = match.MatchId }, match);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found error during match request");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during match request");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending match request");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while sending match request" });
            }
        }

        /// <summary>
        /// Respond to a match request
        /// </summary>
        [HttpPut("{matchId}/respond")]
        public async Task<ActionResult<MatchDto>> RespondToMatchRequest(Guid matchId, [FromBody] MatchResponseDto responseDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var match = await _matchingService.RespondToMatchRequest(userId, matchId, responseDto);
                return Ok(match);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found error during match response");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during match response");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during match response");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to match request");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while responding to match request" });
            }
        }

        /// <summary>
        /// Cancel a match or unmatch from a buddy
        /// </summary>
        [HttpDelete("{matchId}")]
        public async Task<ActionResult> CancelMatch(Guid matchId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                await _matchingService.CancelMatch(userId, matchId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found error during match cancellation");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during match cancellation");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during match cancellation");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling match");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while canceling match" });
            }
        }

        /// <summary>
        /// Get a specific match by ID
        /// </summary>
        [HttpGet("{matchId}")]
        public async Task<ActionResult<MatchDto>> GetMatchById(Guid matchId)
        {
            try
            {
                var match = await _matchingService.GetMatchById(matchId);
                return Ok(match);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Match not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting match by ID");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while fetching match" });
            }
        }
    }
}