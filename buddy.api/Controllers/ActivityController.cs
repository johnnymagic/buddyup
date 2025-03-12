using System;
using System.Collections.Generic;
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
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;
        private readonly ILogger<ActivityController> _logger;

        public ActivityController(
            IActivityService activityService,
            ILogger<ActivityController> logger)
        {
            _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetActivities([FromQuery] ActivitySearchDto searchDto)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "User ID not found" });
                }

                var activities = await _activityService.GetActivities(userId, searchDto);
                return Ok(new ApiResponse { Success = true, Data = activities });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activities");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while retrieving activities" });
            }
        }

        [HttpGet("{activityId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetActivityById(Guid activityId)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                var activity = await _activityService.GetActivityById(activityId, userId);
                return Ok(new ApiResponse { Success = true, Data = activity });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting activity {activityId}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while retrieving the activity" });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> CreateActivity([FromBody] ActivityCreateDto activityDto)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "User ID not found" });
                }

                var createdActivity = await _activityService.CreateActivity(userId, activityDto);
                return CreatedAtAction(nameof(GetActivityById), new { activityId = createdActivity.ActivityId }, 
                    new ApiResponse { Success = true, Data = createdActivity });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while creating the activity" });
            }
        }

        [HttpPut("{activityId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> UpdateActivity(Guid activityId, [FromBody] ActivityUpdateDto activityDto)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "User ID not found" });
                }

                var updatedActivity = await _activityService.UpdateActivity(activityId, userId, activityDto);
                return Ok(new ApiResponse { Success = true, Data = updatedActivity });
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
                _logger.LogError(ex, $"Error updating activity {activityId}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while updating the activity" });
            }
        }

        [HttpDelete("{activityId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteActivity(Guid activityId)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "User ID not found" });
                }

                await _activityService.DeleteActivity(activityId, userId);
                return NoContent();
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
                _logger.LogError(ex, $"Error deleting activity {activityId}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while deleting the activity" });
            }
        }

        [HttpGet("my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetUserActivities()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "User ID not found" });
                }

                var activities = await _activityService.GetUserActivities(userId);
                return Ok(new ApiResponse { Success = true, Data = activities });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user activities");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while retrieving activities" });
            }
        }

        [HttpGet("nearby")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<ActivityDto>>> GetNearbyActivities(
            [FromQuery] double latitude, 
            [FromQuery] double longitude, 
            [FromQuery] double distance = 10, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var response = await _activityService.GetNearbyActivities(latitude, longitude, distance, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nearby activities");
                return BadRequest(new PaginatedResponse<ActivityDto> { Success = false, Message = "An error occurred while retrieving nearby activities" });
            }
        }

        [HttpGet("recommended")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetRecommendedActivities()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "User ID not found" });
                }

                var activities = await _activityService.GetRecommendedActivities(userId);
                return Ok(new ApiResponse { Success = true, Data = activities });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended activities");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while retrieving recommended activities" });
            }
        }
    }
}