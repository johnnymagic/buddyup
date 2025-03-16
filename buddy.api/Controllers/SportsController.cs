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
    [ApiController]
    [Route("api/[controller]")]
    public class SportsController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<SportsController> _logger;

        public SportsController(
            IAdminService adminService,
            ILogger<SportsController> logger)
        {
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> GetSports()
        {
            try
            {
                var sports = await _adminService.GetSports();
                return Ok(new ApiResponse { Success = true, Data = sports });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sports");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while retrieving sports" });
            }
        }

        // [Authorize(Roles = "admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> AddSport([FromBody] SportCreateDto sportDto)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "User ID not found" });
                }

                var sport = await _adminService.AddSport(userId, sportDto);
                return CreatedAtAction(nameof(GetSports), new ApiResponse { Success = true, Data = sport });
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
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding sport");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while adding the sport" });
            }
        }

        // [Authorize(Roles = "admin")]
        [HttpPut("{sportId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> UpdateSport(Guid sportId, [FromBody] SportUpdateDto sportDto)
        {
            try
            {
                var sport = await _adminService.UpdateSport(sportId, sportDto);
                return Ok(new ApiResponse { Success = true, Data = sport });
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
                _logger.LogError(ex, $"Error updating sport {sportId}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while updating the sport" });
            }
        }
    }
}