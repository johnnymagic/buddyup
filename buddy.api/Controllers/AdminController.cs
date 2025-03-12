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
    [Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAdminService adminService,
            ILogger<AdminController> logger)
        {
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> GetDashboardStats()
        {
            try
            {
                var stats = await _adminService.GetDashboardStats();
                return Ok(new ApiResponse { Success = true, Data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while retrieving dashboard statistics" });
            }
        }

        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<UserListItemDto>>> GetUsers([FromQuery] UserSearchDto searchDto)
        {
            try
            {
                var users = await _adminService.GetUsers(searchDto);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users in admin controller");
                return BadRequest(new PaginatedResponse<UserListItemDto> { Success = false, Message = "An error occurred while retrieving users" });
            }
        }

        [HttpPut("users/{userId:guid}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> UpdateUserStatus(Guid userId, [FromBody] UserStatusUpdateDto statusDto)
        {
            try
            {
                var user = await _adminService.UpdateUserStatus(userId, statusDto.Active);
                return Ok(new ApiResponse { Success = true, Data = user });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user status for user {userId}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while updating user status" });
            }
        }

        [HttpPut("users/{userId:guid}/admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> SetAdminStatus(Guid userId, [FromBody] AdminStatusUpdateDto statusDto)
        {
            try
            {
                var user = await _adminService.SetAdminStatus(userId, statusDto.IsAdmin);
                return Ok(new ApiResponse { Success = true, Data = user });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting admin status for user {userId}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while setting admin status" });
            }
        }

        [HttpGet("reports")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<UserReportDto>>> GetUserReports(
            [FromQuery] string status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var reports = await _adminService.GetUserReports(status, page, pageSize);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user reports");
                return BadRequest(new PaginatedResponse<UserReportDto> { Success = false, Message = "An error occurred while retrieving user reports" });
            }
        }

        [HttpPut("reports/{reportId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> HandleUserReport(
            Guid reportId,
            [FromBody] UserReportUpdateDto updateDto)
        {
            try
            {
                var adminUserId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(adminUserId))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "Admin user ID not found" });
                }

                var report = await _adminService.HandleUserReport(reportId, adminUserId, updateDto);
                return Ok(new ApiResponse { Success = true, Data = report });
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
                _logger.LogError(ex, $"Error handling user report {reportId}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while handling user report" });
            }
        }

        [HttpDelete("content/{contentType}/{contentId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteContent(
            string contentType,
            Guid contentId,
            [FromBody] ContentDeletionDto deletionDto)
        {
            try
            {
                var adminUserId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(adminUserId))
                {
                    return BadRequest(new ApiResponse { Success = false, Message = "Admin user ID not found" });
                }

                await _adminService.DeleteContent(contentType, contentId, adminUserId, deletionDto.Reason);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access");
                return Unauthorized(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {contentType} {contentId}");
                return BadRequest(new ApiResponse { Success = false, Message = "An error occurred while deleting content" });
            }
        }
    }

    // These classes are needed for request bodies
    public class UserStatusUpdateDto
    {
        public bool Active { get; set; }
    }

    public class AdminStatusUpdateDto
    {
        public bool IsAdmin { get; set; }
    }

    public class ContentDeletionDto
    {
        public string Reason { get; set; }
    }
}