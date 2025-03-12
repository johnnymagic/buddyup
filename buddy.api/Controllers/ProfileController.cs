using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using BuddyUp.API.Models.Domain;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Models.Responses;
using BuddyUp.API.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BuddyUp.API.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IUserService _userService;

        public ProfileController(IProfileService profileService, IUserService userService)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        /// Get the current user's profile
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ProfileDto>> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Success = false, Message = "User not authenticated" });
            }

            var profile = await _profileService.GetProfileByUserId(userId);
            if (profile == null)
            {
                return NotFound(new ApiResponse { Success = false, Message = "Profile not found" });
            }

            return Ok(profile);
        }

        /// <summary>
        /// Create or update the user's profile
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProfileDto>> CreateProfile([FromBody] ProfileDto profileDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
            }

            // Check if profile already exists
            var existingProfile = await _profileService.GetProfileByUserId(userId);
            if (existingProfile != null)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Profile already exists. Use PUT to update." });
            }

            // Create user if not exists
            var existingUser = await _userService.GetUserByAuth0Id(userId);
            if (existingUser == null)
            {
                var userDto = new UserDto
                {
                    Auth0Id = userId,
                    Email = profileDto.Email,
                    FirstName = profileDto.FirstName,
                    LastName = profileDto.LastName
                };

                await _userService.CreateUser(userDto);
            }

            profileDto.UserId = userId;
            var createdProfile = await _profileService.CreateProfile(profileDto);
            return CreatedAtAction(nameof(GetProfile), new { id = createdProfile.ProfileId }, createdProfile);
        }

        /// <summary>
        /// Update the user's profile
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<ProfileDto>> UpdateProfile([FromBody] ProfileDto profileDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
            }

            // Check if profile exists
            var existingProfile = await _profileService.GetProfileByUserId(userId);
            if (existingProfile == null)
            {
                return NotFound(new ApiResponse { Success = false, Message = "Profile not found. Use POST to create." });
            }

            profileDto.UserId = userId;
            profileDto.ProfileId = existingProfile.ProfileId; // Ensure we're updating the right profile
            var updatedProfile = await _profileService.UpdateProfile(profileDto);
            return Ok(updatedProfile);
        }

        /// <summary>
        /// Get the current user's sports
        /// </summary>
        [HttpGet("sports")]
        public async Task<ActionResult<IEnumerable<UserSportDto>>> GetUserSports()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
            }

            var sports = await _profileService.GetUserSports(userId);
            return Ok(sports);
        }

        /// <summary>
        /// Add a sport to the user's profile
        /// </summary>
        [HttpPost("sports")]
        public async Task<ActionResult<UserSportDto>> AddUserSport([FromBody] UserSportDto userSportDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
            }

            userSportDto.UserId = userId;
            var addedSport = await _profileService.AddUserSport(userSportDto);
            return CreatedAtAction(nameof(GetUserSports), null, addedSport);
        }

        /// <summary>
        /// Update a user's sport skill level
        /// </summary>
        [HttpPut("sports/{userSportId}")]
        public async Task<ActionResult<UserSportDto>> UpdateUserSport(Guid userSportId, [FromBody] UserSportDto userSportDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
            }

            // Ensure the userSportId is set correctly
            userSportDto.UserSportId = userSportId;
            userSportDto.UserId = userId;

            // Verify ownership
            var existingSport = await _profileService.GetUserSportById(userSportId);
            if (existingSport == null || existingSport.UserId != userId)
            {
                return NotFound(new ApiResponse { Success = false, Message = "Sport not found or does not belong to user" });
            }

            var updatedSport = await _profileService.UpdateUserSport(userSportDto);
            return Ok(updatedSport);
        }

        /// <summary>
        /// Remove a sport from the user's profile
        /// </summary>
        [HttpDelete("sports/{userSportId}")]
        public async Task<ActionResult> RemoveUserSport(Guid userSportId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
            }

            // Verify ownership
            var existingSport = await _profileService.GetUserSportById(userSportId);
            if (existingSport == null || existingSport.UserId != userId)
            {
                return NotFound(new ApiResponse { Success = false, Message = "Sport not found or does not belong to user" });
            }

            await _profileService.RemoveUserSport(userSportId);
            return NoContent();
        }

        /// <summary>
        /// Get the verification status of the current user
        /// </summary>
        [HttpGet("verification")]
        public async Task<ActionResult<VerificationDto>> GetVerificationStatus()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
            }

            var verification = await _profileService.GetVerificationStatus(userId);
            return Ok(verification);
        }

        /// <summary>
        /// Initiate verification process for the user
        /// </summary>
        [HttpPost("verification/initiate")]
        public async Task<ActionResult<VerificationDto>> InitiateVerification([FromBody] VerificationRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
            }

            request.UserId = userId;
            var verification = await _profileService.InitiateVerification(request);
            return Ok(verification);
        }
    }
}