using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BuddyUp.API.Data.Repositories;
using BuddyUp.API.Models.Domain;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Models.Responses;
using BuddyUp.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuddyUp.API.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Sport> _sportRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<UserReport> _reportRepository;
        private readonly IRepository<Match> _matchRepository;
        private readonly IRepository<Verification> _verificationRepository;
        private readonly IRepository<Message> _messageRepository;
        private readonly ILogger<AdminService> _logger;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public AdminService(
            IRepository<User> userRepository,
            IRepository<Sport> sportRepository,
            IRepository<Location> locationRepository,
            IRepository<UserReport> reportRepository,
            IRepository<Match> matchRepository,
            IRepository<Verification> verificationRepository,
            IRepository<Message> messageRepository,
            ILogger<AdminService> logger,
            IMapper mapper,
            IUserService userService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _sportRepository = sportRepository ?? throw new ArgumentNullException(nameof(sportRepository));
            _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
            _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
            _matchRepository = matchRepository ?? throw new ArgumentNullException(nameof(matchRepository));
            _verificationRepository = verificationRepository ?? throw new ArgumentNullException(nameof(verificationRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<DashboardStatsDto> GetDashboardStats()
        {
            try
            {
                // User statistics
                var totalUsers = await _userRepository.CountAsync();

                var today = DateTime.UtcNow.Date;
                var newUsersToday = await _userRepository.Query()
                    .CountAsync(u => u.CreatedAt.Date == today);

                // Sport statistics
                var totalSports = await _sportRepository.CountAsync();
                var activeSports = await _sportRepository.CountAsync(s => s.IsActive);

                // Match statistics
                var totalMatches = await _matchRepository.CountAsync();
                var pendingMatches = await _matchRepository.CountAsync(m => m.Status == "Pending");
                var acceptedMatches = await _matchRepository.CountAsync(m => m.Status == "Accepted");
                var rejectedMatches = await _matchRepository.CountAsync(m => m.Status == "Rejected");

                // Verification statistics
                var totalVerified = await _userRepository.CountAsync(u => u.IsVerified);
                var pendingVerifications = await _verificationRepository.CountAsync(v => v.Status == "Pending");
                var verificationRate = totalUsers > 0 ? (double)totalVerified / totalUsers * 100 : 0;

                // User growth over last 6 months
                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
                var userGrowth = await _userRepository.Query()
                    .Where(u => u.CreatedAt >= sixMonthsAgo)
                    .GroupBy(u => new
                    {
                        Year = u.CreatedAt.Year,
                        Month = u.CreatedAt.Month
                    })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => new PeriodDataDto
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Count = g.Count()
                    })
                    .ToListAsync();

                // Most popular sports
                var sportPopularity = await _userRepository.Query()
                    .SelectMany(u => u.Sports)
                    .GroupBy(us => us.Sport.Name)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => new SportPopularityDto
                    {
                        Name = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                // Recent activity (last 10 significant events)
                var recentActivity = new List<RecentActivityDto>();

                // New users
                var recentUsers = await _userRepository.Query()
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .Select(u => new RecentActivityDto
                    {
                        Type = "user",
                        Description = $"New user {u.FirstName} {u.LastName} joined",
                        Time = u.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                    })
                    .ToListAsync();

                // Recent matches
                var recentMatches = await _matchRepository.Query()
                    .Include(m => m.Requester)
                    .Include(m => m.Recipient)
                    .OrderByDescending(m => m.Status == "Accepted" ? m.RespondedAt : m.RequestedAt)
                    .Take(5)
                    .Select(m => new RecentActivityDto
                    {
                        Type = "match",
                        Description = m.Status == "Accepted"
                            ? $"{m.Requester.FirstName} and {m.Recipient.FirstName} are now workout buddies"
                            : $"{m.Requester.FirstName} sent a buddy request to {m.Recipient.FirstName}",
                        Time = (m.Status == "Accepted"
                            ? (m.RespondedAt.HasValue ? m.RespondedAt.Value : DateTime.MinValue)
                            : m.RequestedAt).ToString("yyyy-MM-dd HH:mm")
                    })
                    .ToListAsync();

                // Recent verifications
                var recentVerifications = await _verificationRepository.Query()
                    .Include(v => v.User)
                    .Where(v => v.Status == "Completed")
                    .OrderByDescending(v => v.CompletedAt)
                    .Take(5)
                    .Select(v => new RecentActivityDto
                    {
                        Type = "verification",
                        Description = $"{v.User.FirstName} {v.User.LastName} was verified",
                        Time = v.CompletedAt.HasValue ? v.CompletedAt.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty
                    })
                    .ToListAsync();

                // Combine and sort all activities
                recentActivity.AddRange(recentUsers);
                recentActivity.AddRange(recentMatches);
                recentActivity.AddRange(recentVerifications);

                recentActivity = recentActivity
                    .OrderByDescending(a => DateTime.Parse(a.Time))
                    .Take(10)
                    .ToList();

                return new DashboardStatsDto
                {
                    TotalUsers = totalUsers,
                    NewUsersToday = newUsersToday,
                    TotalSports = totalSports,
                    ActiveSports = activeSports,
                    MatchStats = new MatchStatsDto
                    {
                        TotalMatches = totalMatches,
                        PendingMatches = pendingMatches,
                        AcceptedMatches = acceptedMatches,
                        RejectedMatches = rejectedMatches
                    },
                    VerificationStats = new VerificationStatsDto
                    {
                        TotalVerified = totalVerified,
                        PendingVerifications = pendingVerifications,
                        VerificationRate = Math.Round(verificationRate, 2)
                    },
                    UserGrowth = userGrowth,
                    SportPopularity = sportPopularity,
                    RecentActivity = recentActivity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                throw;
            }
        }

        public async Task<PaginatedResponse<UserListItemDto>> GetUsers(UserSearchDto searchDto)
        {
            try
            {
                // Delegate to the user service
                return await _userService.GetUsers(searchDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users in admin service");
                throw;
            }
        }

        public async Task<UserDto> UpdateUserStatus(Guid userId, bool active)
        {
            try
            {
                // Delegate to the user service
                return await _userService.UpdateUserStatus(userId, active);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user status for user {userId} in admin service");
                throw;
            }
        }

        public async Task<UserDto> SetAdminStatus(Guid userId, bool isAdmin)
        {
            try
            {
                // Delegate to the user service
                return await _userService.SetAdminStatus(userId, isAdmin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting admin status for user {userId} in admin service");
                throw;
            }
        }

        public async Task<IEnumerable<SportDto>> GetSports()
        {
            try
            {
                var sports = await _sportRepository.Query()
                    .Include(s => s.CreatedBy)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<SportDto>>(sports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sports in admin service");
                throw;
            }
        }

        public async Task<SportDto> AddSport(string userId, SportCreateDto sportDto)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                if (!user.IsAdmin)
                {
                    throw new UnauthorizedAccessException("Only administrators can add sports");
                }

                // Check if sport name already exists
                var existingSport = await _sportRepository.Query()
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == sportDto.Name.ToLower());

                if (existingSport != null)
                {
                    throw new InvalidOperationException($"Sport with name '{sportDto.Name}' already exists");
                }

                var sport = _mapper.Map<Sport>(sportDto);
                sport.SportId = Guid.NewGuid();
                sport.CreatedAt = DateTime.UtcNow;
                sport.CreatedByUserId = user.UserId;
                sport.IsActive = true;

                await _sportRepository.AddAsync(sport);
                await _sportRepository.SaveChangesAsync();

                // Fetch the complete sport with navigation properties
                sport = await _sportRepository.Query()
                    .Include(s => s.CreatedBy)
                    .FirstOrDefaultAsync(s => s.SportId == sport.SportId);

                return _mapper.Map<SportDto>(sport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding sport by user {userId} in admin service");
                throw;
            }
        }

        public async Task<SportDto> UpdateSport(Guid sportId, SportUpdateDto sportDto)
        {
            try
            {
                var sport = await _sportRepository.Query()
                    .Include(s => s.CreatedBy)
                    .FirstOrDefaultAsync(s => s.SportId == sportId);

                if (sport == null)
                {
                    throw new KeyNotFoundException($"Sport not found with ID: {sportId}");
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(sportDto.Name))
                {
                    // Check if the new name already exists in another sport
                    var existingSport = await _sportRepository.Query()
                        .FirstOrDefaultAsync(s => s.Name.ToLower() == sportDto.Name.ToLower() && s.SportId != sportId);

                    if (existingSport != null)
                    {
                        throw new InvalidOperationException($"Sport with name '{sportDto.Name}' already exists");
                    }

                    sport.Name = sportDto.Name;
                }

                if (sportDto.Description != null) // Allow clearing description
                {
                    sport.Description = sportDto.Description;
                }

                if (sportDto.IconUrl != null) // Allow clearing icon URL
                {
                    sport.IconUrl = sportDto.IconUrl;
                }

                if (sportDto.IsActive.HasValue)
                {
                    sport.IsActive = sportDto.IsActive.Value;
                }

                await _sportRepository.UpdateAsync(sport);
                await _sportRepository.SaveChangesAsync();

                return _mapper.Map<SportDto>(sport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating sport {sportId} in admin service");
                throw;
            }
        }

        public async Task<PaginatedResponse<LocationDto>> GetLocations(int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _locationRepository.Query()
                    .Include(l => l.CreatedBy)
                    .OrderBy(l => l.Name);

                var totalItems = await query.CountAsync();

                var locations = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var locationDtos = _mapper.Map<List<LocationDto>>(locations);

                return new PaginatedResponse<LocationDto>
                {
                    Success = true,
                    Items = locationDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting locations in admin service");
                throw;
            }
        }

        public async Task<LocationDto> AddLocation(string userId, LocationCreateDto locationDto)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                // Verify admin status for certain operations
                if (!user.IsAdmin && !string.IsNullOrEmpty(locationDto.City) && !string.IsNullOrEmpty(locationDto.State))
                {
                    // For non-admins, check if similar location already exists
                    var existingLocation = await _locationRepository.Query()
                        .FirstOrDefaultAsync(l =>
                            l.Name.ToLower() == locationDto.Name.ToLower() &&
                            l.City.ToLower() == locationDto.City.ToLower() &&
                            l.State.ToLower() == locationDto.State.ToLower());

                    if (existingLocation != null)
                    {
                        throw new InvalidOperationException($"Similar location already exists");
                    }
                }

                var location = _mapper.Map<Location>(locationDto);
                location.LocationId = Guid.NewGuid();
                location.CreatedAt = DateTime.UtcNow;
                location.CreatedByUserId = user.UserId;
                location.IsActive = true;
                location.IsVerified = user.IsAdmin; // Auto-verify locations added by admins

                await _locationRepository.AddAsync(location);
                await _locationRepository.SaveChangesAsync();

                // Fetch the complete location with navigation properties
                location = await _locationRepository.Query()
                    .Include(l => l.CreatedBy)
                    .FirstOrDefaultAsync(l => l.LocationId == location.LocationId);

                return _mapper.Map<LocationDto>(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding location by user {userId} in admin service");
                throw;
            }
        }

        public async Task<LocationDto> UpdateLocation(Guid locationId, LocationUpdateDto locationDto)
        {
            try
            {
                var location = await _locationRepository.Query()
                    .Include(l => l.CreatedBy)
                    .FirstOrDefaultAsync(l => l.LocationId == locationId);

                if (location == null)
                {
                    throw new KeyNotFoundException($"Location not found with ID: {locationId}");
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(locationDto.Name))
                {
                    location.Name = locationDto.Name;
                }

                if (locationDto.Address != null) // Allow clearing address
                {
                    location.Address = locationDto.Address;
                }

                if (!string.IsNullOrEmpty(locationDto.City))
                {
                    location.City = locationDto.City;
                }

                if (!string.IsNullOrEmpty(locationDto.State))
                {
                    location.State = locationDto.State;
                }

                if (locationDto.PostalCode != null) // Allow clearing postal code
                {
                    location.PostalCode = locationDto.PostalCode;
                }

                if (!string.IsNullOrEmpty(locationDto.Country))
                {
                    location.Country = locationDto.Country;
                }

                if (locationDto.Latitude.HasValue && locationDto.Longitude.HasValue)
                {
                    location.Coordinates = new NetTopologySuite.Geometries.Point(locationDto.Longitude.Value, locationDto.Latitude.Value)
                    {
                        SRID = 4326
                    };
                }

                if (locationDto.IsVerified.HasValue)
                {
                    location.IsVerified = locationDto.IsVerified.Value;
                }

                if (locationDto.IsActive.HasValue)
                {
                    location.IsActive = locationDto.IsActive.Value;
                }

                await _locationRepository.UpdateAsync(location);
                await _locationRepository.SaveChangesAsync();

                return _mapper.Map<LocationDto>(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating location {locationId} in admin service");
                throw;
            }
        }

        public async Task<PaginatedResponse<UserReportDto>> GetUserReports(string status = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _reportRepository.Query()
                    .Include(r => r.ReportingUser)
                    .Include(r => r.ReportedUser)
                    .Include(r => r.ReviewedBy)
                    .AsQueryable();

                // Filter by status if provided
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(r => r.Status == status);
                }

                // Order by creation date, newest first
                query = query.OrderByDescending(r => r.CreatedAt);

                var totalItems = await query.CountAsync();

                var reports = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var reportDtos = new List<UserReportDto>();
                foreach (var report in reports)
                {
                    var dto = new UserReportDto
                    {
                        ReportId = report.ReportId,
                        ReportingUserId = report.ReportingUserId,
                        ReportingUserName = $"{report.ReportingUser.FirstName} {report.ReportingUser.LastName}",
                        ReportedUserId = report.ReportedUserId,
                        ReportedUserName = $"{report.ReportedUser.FirstName} {report.ReportedUser.LastName}",
                        Reason = report.Reason,
                        Status = report.Status,
                        CreatedAt = report.CreatedAt,
                        ReviewedAt = report.ReviewedAt
                    };

                    if (report.ReviewedBy != null)
                    {
                        dto.ReviewedBy = $"{report.ReviewedBy.FirstName} {report.ReviewedBy.LastName}";
                    }

                    reportDtos.Add(dto);
                }

                return new PaginatedResponse<UserReportDto>
                {
                    Success = true,
                    Items = reportDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user reports with status {status} in admin service");
                throw;
            }
        }

        public async Task<UserReportDto> HandleUserReport(Guid reportId, string adminUserId, UserReportUpdateDto updateDto)
        {
            try
            {
                var adminUser = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == adminUserId);

                if (adminUser == null)
                {
                    throw new KeyNotFoundException($"Admin user not found with ID: {adminUserId}");
                }

                if (!adminUser.IsAdmin)
                {
                    throw new UnauthorizedAccessException("Only administrators can handle user reports");
                }

                var report = await _reportRepository.Query()
                    .Include(r => r.ReportingUser)
                    .Include(r => r.ReportedUser)
                    .FirstOrDefaultAsync(r => r.ReportId == reportId);

                if (report == null)
                {
                    throw new KeyNotFoundException($"Report not found with ID: {reportId}");
                }

                // Update report status
                report.Status = updateDto.Status;
                report.ReviewedAt = DateTime.UtcNow;
                report.ReviewedByUserId = adminUser.UserId;

                await _reportRepository.UpdateAsync(report);
                await _reportRepository.SaveChangesAsync();

                // If taking action on the reported user (e.g., account deactivation)
                if (updateDto.Status == "Action Taken")
                {
                    var reportedUser = report.ReportedUser;
                    reportedUser.Active = false; // Deactivate the reported user
                    await _userRepository.UpdateAsync(reportedUser);
                    await _userRepository.SaveChangesAsync();
                }

                var dto = new UserReportDto
                {
                    ReportId = report.ReportId,
                    ReportingUserId = report.ReportingUserId,
                    ReportingUserName = $"{report.ReportingUser.FirstName} {report.ReportingUser.LastName}",
                    ReportedUserId = report.ReportedUserId,
                    ReportedUserName = $"{report.ReportedUser.FirstName} {report.ReportedUser.LastName}",
                    Reason = report.Reason,
                    Status = report.Status,
                    CreatedAt = report.CreatedAt,
                    ReviewedAt = report.ReviewedAt,
                    ReviewedBy = $"{adminUser.FirstName} {adminUser.LastName}"
                };

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling user report {reportId} by admin {adminUserId}");
                throw;
            }
        }

        public async Task DeleteContent(string contentType, Guid contentId, string adminUserId, string reason)
        {
            try
            {
                var adminUser = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == adminUserId);

                if (adminUser == null)
                {
                    throw new KeyNotFoundException($"Admin user not found with ID: {adminUserId}");
                }

                if (!adminUser.IsAdmin)
                {
                    throw new UnauthorizedAccessException("Only administrators can delete content");
                }

                switch (contentType.ToLower())
                {
                    case "message":
                        await DeleteMessage(contentId, reason);
                        break;
                    case "sport":
                        await DeleteSport(contentId, reason);
                        break;
                    case "location":
                        await DeleteLocation(contentId, reason);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported content type: {contentType}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {contentType} {contentId} by admin {adminUserId}");
                throw;
            }
        }

        private async Task DeleteMessage(Guid messageId, string reason)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
            {
                throw new KeyNotFoundException($"Message not found with ID: {messageId}");
            }

            // Soft delete by marking as deleted and clearing content
            message.IsDeleted = true;
            message.Content = $"[Removed by admin: {reason}]";
            await _messageRepository.UpdateAsync(message);
            await _messageRepository.SaveChangesAsync();
        }

        private async Task DeleteSport(Guid sportId, string reason)
        {
            var sport = await _sportRepository.GetByIdAsync(sportId);
            if (sport == null)
            {
                throw new KeyNotFoundException($"Sport not found with ID: {sportId}");
            }

            // Check if sport is in use
            var usersWithSport = await _userRepository.Query()
                .SelectMany(u => u.Sports)
                .AnyAsync(us => us.SportId == sportId);

            if (usersWithSport)
            {
                // Soft delete by deactivating
                sport.IsActive = false;
                await _sportRepository.UpdateAsync(sport);
            }
            else
            {
                // Hard delete if not in use
                await _sportRepository.DeleteAsync(sport);
            }

            await _sportRepository.SaveChangesAsync();
        }

        private async Task DeleteLocation(Guid locationId, string reason)
        {
            var location = await _locationRepository.GetByIdAsync(locationId);
            if (location == null)
            {
                throw new KeyNotFoundException($"Location not found with ID: {locationId}");
            }

            // Soft delete by deactivating
            location.IsActive = false;
            await _locationRepository.UpdateAsync(location);
            await _locationRepository.SaveChangesAsync();
        }
    }
}