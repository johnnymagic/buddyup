using System;
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
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get current user info
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
                }

                var user = await _userService.GetUserByAuth0Id(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while getting user information" });
            }
        }

        /// <summary>
        /// Record a login event
        /// </summary>
        [HttpPost("login")]
        [Authorize]
        public async Task<ActionResult> RecordLogin()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
                }

                await _userService.UpdateLastLogin(userId);
                return Ok(new ApiResponse { Success = true, Message = "Login recorded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording login");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while recording login" });
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [Authorize]
        public async Task<ActionResult<UserDto>> RegisterUser([FromBody] UserDto userDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
                }

                // Check if user already exists
                var existingUser = await _userService.GetUserByAuth0Id(userId);
                if (existingUser != null)
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "User already registered" });
                }

                // Set Auth0 ID from claims
                userDto.Auth0Id = userId;

                // Create the user
                var createdUser = await _userService.CreateUser(userDto);
                return CreatedAtAction(nameof(GetCurrentUser), createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while registering user" });
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        [HttpPut("update")]
        [Authorize]
        public async Task<ActionResult<UserDto>> UpdateUser([FromBody] UserDto userDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
                }

                // Get existing user to verify ownership
                var existingUser = await _userService.GetUserByAuth0Id(userId);
                if (existingUser == null)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "User not found" });
                }

                // Ensure we're updating the right user
                userDto.Auth0Id = userId;
                userDto.UserId = existingUser.UserId;

                // Update user
                var updatedUser = await _userService.UpdateUser(userDto);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while updating user" });
            }
        }
    }
}