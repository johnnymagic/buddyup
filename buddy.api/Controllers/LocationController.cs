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
    public class LocationController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<LocationController> _logger;

        public LocationController(
            IAdminService adminService,
            ILogger<LocationController> logger)
        {
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<LocationDto>>> GetLocations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var locations = await _adminService.GetLocations(page, pageSize);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting locations");
                return BadRequest(new PaginatedResponse<LocationDto> { Success = false, Message = "An error occurred while retrieving locations" });
            }
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> AddLocation([FromBody] LocationCreateDto locationDto)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "User ID not found" });
                }

                var location = await _adminService.AddLocation(userId, locationDto);
                return CreatedAtAction(nameof(GetLocations), new ApiResponse { Success = true, Data = location });
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
                _logger.LogError(ex, "Error adding location");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while adding the location" });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{locationId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> UpdateLocation(Guid locationId, [FromBody] LocationUpdateDto locationDto)
        {
            try
            {
                var location = await _adminService.UpdateLocation(locationId, locationDto);
                return Ok(new ApiResponse { Success = true, Data = location });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating location {locationId}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while updating the location" });
            }
        }
    }
}