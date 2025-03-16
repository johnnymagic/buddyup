using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Routing;
using BuddyUp.API.Models.Domain;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Models.Responses;
using BuddyUp.API.Services.Interfaces;
using BuddyUp.API.Services;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;

namespace BuddyUp.API.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IUserService _userService;
        private readonly ILogger<ProfileController> _logger;
        private readonly ISportService _sportService;
        public ProfileController(IProfileService profileService, IUserService userService, ILogger<ProfileController> logger, ISportService sportService)
        {
            _sportService = sportService ?? throw new ArgumentNullException(nameof(sportService));
       

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        /// Get the current user's profile
        /// </summary>
        /// <summary>
        /// Get the current user's profile
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ProfileDto>> GetProfile()
        {
            var auth0UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(auth0UserId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
            }

            var profile = await _profileService.GetByAuth0Id(auth0UserId);
            if (profile == null)
            {
                return NotFound(new ApiResponse { Success = false, Message = "Profile not found" });
            }

            return Ok(profile);
        }

        // Fix for the ProfileController UpsertProfile method


        [HttpPut]
        public async Task<ActionResult<ProfileDto>> UpsertProfile([FromBody] ProfileUpdateInputDto inputDto)
        {
            if (inputDto == null)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Profile data is required" });
            }

            // Get the Auth0 user ID from the token
            var auth0UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(auth0UserId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
            }

            _logger.LogInformation($"Upserting profile for Auth0 user: {auth0UserId}");

            try
            {
                // Check if profile already exists
                var existingProfile = await _profileService.GetByAuth0Id(auth0UserId);
                ProfileDto result;

                if (existingProfile == null)
                {
                    // Profile doesn't exist - create a new one
                    _logger.LogInformation($"Profile not found, creating new profile for Auth0 user: {auth0UserId}");

                    var profileDto = new ProfileDto
                    {
                        Auth0UserId = auth0UserId,
                        Email = inputDto.Email,
                        FirstName = inputDto.FirstName,
                        LastName = inputDto.LastName,
                        Bio = inputDto.Bio ?? string.Empty,
                        ProfilePictureUrl = inputDto.ProfilePictureUrl ?? string.Empty,
                        MaxTravelDistance = inputDto.MaxTravelDistance,
                        PreferredDays = inputDto.PreferredDays ?? new List<string>(),
                        PreferredTimes = inputDto.PreferredTimes ?? new List<string>(),
                        PublicProfile = inputDto.PublicProfile
                    };

                    // Set location if provided
                    if (inputDto.Latitude.HasValue && inputDto.Longitude.HasValue)
                    {
                        profileDto.Latitude = inputDto.Latitude;
                        profileDto.Longitude = inputDto.Longitude;
                    }

                    result = await _profileService.CreateProfile(profileDto);
                    _logger.LogInformation($"Created new profile with ID {result.ProfileId}");
                }
                else
                {
                    // Profile exists - update it
                    _logger.LogInformation($"Updating existing profile for Auth0 user: {auth0UserId}");

                    existingProfile.FirstName = inputDto.FirstName ?? existingProfile.FirstName;
                    existingProfile.LastName = inputDto.LastName ?? existingProfile.LastName;
                    existingProfile.Email = inputDto.Email ?? existingProfile.Email;
                    existingProfile.Bio = inputDto.Bio ?? existingProfile.Bio;
                    existingProfile.ProfilePictureUrl = inputDto.ProfilePictureUrl ?? existingProfile.ProfilePictureUrl;
                    existingProfile.MaxTravelDistance = inputDto.MaxTravelDistance;
                    existingProfile.PreferredDays = inputDto.PreferredDays ?? existingProfile.PreferredDays;
                    existingProfile.PreferredTimes = inputDto.PreferredTimes ?? existingProfile.PreferredTimes;
                    existingProfile.PublicProfile = inputDto.PublicProfile;

                    // Update location if provided
                    if (inputDto.Latitude.HasValue && inputDto.Longitude.HasValue)
                    {
                        existingProfile.Latitude = inputDto.Latitude;
                        existingProfile.Longitude = inputDto.Longitude;
                    }

                    result = await _profileService.UpdateProfile(existingProfile);
                    _logger.LogInformation($"Updated existing profile with ID {result.ProfileId}");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error upserting profile for Auth0 user {auth0UserId}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = $"Failed to upsert profile: {ex.Message}"
                });
            }
        }



        [HttpPost]
        public async Task<ActionResult<ProfileDto>> CreateProfile([FromBody] ProfileUpdateInputDto inputDto)
        {
            // The same implementation as before
            if (inputDto == null)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Profile data is required" });
            }

            // Get the Auth0 user ID from the token
            var auth0UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(auth0UserId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated" });
            }

            _logger.LogInformation($"Creating profile for Auth0 user: {auth0UserId}");

            // Check if profile already exists
            var existingProfile = await _profileService.GetByAuth0Id(auth0UserId);
            if (existingProfile != null)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Profile already exists. Use PUT to update." });
            }

            // Create or get the user first
            var existingUser = await _userService.GetUserByAuth0Id(auth0UserId);
            if (existingUser == null)
            {
                var userDto = new UserDto
                {
                    Auth0Id = auth0UserId,
                    Email = inputDto.Email,
                    FirstName = inputDto.FirstName,
                    LastName = inputDto.LastName
                };

                existingUser = await _userService.CreateUser(userDto);
                if (existingUser == null)
                {
                    return StatusCode(500, new ApiResponse { Success = false, Message = "Failed to create user record" });
                }
            }

            // Create a new ProfileDto from the input
            var profileDto = new ProfileDto
            {
                ProfileId = Guid.NewGuid(),
                UserId = existingUser.UserId,
                Auth0UserId = auth0UserId,
                FirstName = inputDto.FirstName,
                LastName = inputDto.LastName,
                Email = inputDto.Email,
                Bio = inputDto.Bio ?? string.Empty,
                ProfilePictureUrl = inputDto.ProfilePictureUrl ?? string.Empty,
                MaxTravelDistance = inputDto.MaxTravelDistance,
                PreferredDays = inputDto.PreferredDays ?? new List<string>(),
                PreferredTimes = inputDto.PreferredTimes ?? new List<string>(),
                VerificationStatus = inputDto.VerificationStatus ?? "Unverified",
                PublicProfile = inputDto.PublicProfile,
                UpdatedAt = DateTime.UtcNow
            };

            // Create the point from lat/long
            try
            {
                double latitude = inputDto.Latitude ?? 0;
                double longitude = inputDto.Longitude ?? 0;

                var geometryFactory = new GeometryFactory();
                var point = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
                point.SRID = 4326;
                profileDto.PreferredLocation = point;

                _logger.LogInformation($"Created point at ({longitude}, {latitude})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create spatial point, using default");
                var geometryFactory = new GeometryFactory();
                profileDto.PreferredLocation = geometryFactory.CreatePoint(new Coordinate(0, 0));
                profileDto.PreferredLocation.SRID = 4326;
            }

            try
            {
                var createdProfile = await _profileService.CreateProfile(profileDto);
                return CreatedAtAction(nameof(GetProfile), new { id = createdProfile.ProfileId }, createdProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create profile");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = $"Failed to create profile: {ex.Message}"
                });
            }
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

            var user = await _userService.GetUserByAuth0Id(userId);

            // Set the UserId from the token
            userSportDto.UserId = user.UserId.ToString();

            // Get the sport details if not provided
            if (string.IsNullOrEmpty(userSportDto.SportName) || string.IsNullOrEmpty(userSportDto.IconUrl))
            {
                try
                {
                    // Assuming you have a sport service or repository
                    var sport = await _sportService.GetSportById(userSportDto.SportId);
                    if (sport != null)
                    {
                        userSportDto.SportName = sport.Name;
                        userSportDto.IconUrl = sport.IconUrl;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not fetch sport details for SportId {SportId}", userSportDto.SportId);
                }
            }

            // Set defaults for any missing fields
            userSportDto.Notes ??= string.Empty;
            userSportDto.IsPublic = true;
            userSportDto.UpdatedAt = DateTime.UtcNow;

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