using System;
using System.Threading.Tasks;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Models.Responses;
using BuddyUp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuddyUp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationService _verificationService;
        private readonly ILogger<VerificationController> _logger;

        public VerificationController(
            IVerificationService verificationService,
            ILogger<VerificationController> logger)
        {
            _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize]
        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetVerificationStatus()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "User ID not found" });
                }

                var verification = await _verificationService.GetVerificationStatus(userId);
                return Ok(new ApiResponse { Success = true, Data = verification });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verification status");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while retrieving verification status" });
            }
        }

        [Authorize]
        [HttpPost("initiate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> InitiateVerification([FromBody] VerificationRequestDto requestDto)
        {
            try
            {
                // Set the user ID from the authenticated user if not provided
                if (string.IsNullOrEmpty(requestDto.UserId))
                {
                    requestDto.UserId = User.FindFirst("sub")?.Value;
                }

                // Verify the user is either requesting for themselves or is an admin
                var requestingUserId = User.FindFirst("sub")?.Value;
                var isAdmin = User.HasClaim(c => c.Type == "role" && c.Value == "admin");
                
                if (requestDto.UserId != requestingUserId && !isAdmin)
                {
                    return Unauthorized(new ApiResponse { Success = false, Message = "You can only initiate verification for yourself unless you are an admin" });
                }

                var verification = await _verificationService.InitiateVerification(requestDto);
                return Ok(new ApiResponse { Success = true, Data = verification });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating verification");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while initiating verification" });
            }
        }

        [HttpPost("callback/{provider}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> ProcessCallback(string provider, [FromBody] object callbackData)
        {
            try
            {
                var verification = await _verificationService.ProcessVerificationCallback(provider, callbackData);
                return Ok(new ApiResponse { Success = true, Data = verification });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing verification callback from provider {provider}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while processing verification callback" });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpGet("admin/pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<AdminVerificationDto>>> GetPendingVerifications(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var verifications = await _verificationService.GetPendingVerifications(page, pageSize);
                return Ok(verifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending verifications");
                return BadRequest(new PaginatedResponse<AdminVerificationDto> { Success = false, Message = "An error occurred while retrieving pending verifications" });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpGet("admin/list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<AdminVerificationDto>>> GetVerifications(
            [FromQuery] string status = null, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var verifications = await _verificationService.GetVerifications(status, page, pageSize);
                return Ok(verifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verifications");
                return BadRequest(new PaginatedResponse<AdminVerificationDto> { Success = false, Message = "An error occurred while retrieving verifications" });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpGet("admin/{verificationId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetVerificationById(Guid verificationId)
        {
            try
            {
                var verification = await _verificationService.GetVerificationById(verificationId);
                return Ok(new ApiResponse { Success = true, Data = verification });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting verification {verificationId}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while retrieving verification" });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost("admin/complete/{verificationId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> CompleteVerification(
            Guid verificationId, 
            [FromBody] AdminVerificationActionDto actionDto)
        {
            try
            {
                var adminUserId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(adminUserId))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "Admin user ID not found" });
                }

                var verification = await _verificationService.CompleteVerification(
                    verificationId, 
                    actionDto.Approved, 
                    adminUserId, 
                    actionDto.Notes);
                
                return Ok(new ApiResponse { Success = true, Data = verification });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access");
                return Unauthorized(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing verification {verificationId}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while completing verification" });
            }
        }
    }

    // This class is needed for the admin verification action endpoint
    public class AdminVerificationActionDto
    {
        public bool Approved { get; set; }
        public string Notes { get; set; }
    }
}