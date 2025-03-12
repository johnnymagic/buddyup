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
using NetTopologySuite.Geometries;
using Location = BuddyUp.API.Models.Domain.Location;

namespace BuddyUp.API.Services.Implementations
{
    public class ActivityService : IActivityService
    {
        private readonly IRepository<Activity> _activityRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserProfile> _profileRepository;
        private readonly IRepository<UserSport> _userSportRepository;
        private readonly IRepository<Sport> _sportRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly ILogger<ActivityService> _logger;
        private readonly IMapper _mapper;

        public ActivityService(
            IRepository<Activity> activityRepository,
            IRepository<User> userRepository,
            IRepository<UserProfile> profileRepository,
            IRepository<UserSport> userSportRepository,
            IRepository<Sport> sportRepository,
            IRepository<Location> locationRepository,
            ILogger<ActivityService> logger,
            IMapper mapper)
        {
            _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
            _userSportRepository = userSportRepository ?? throw new ArgumentNullException(nameof(userSportRepository));
            _sportRepository = sportRepository ?? throw new ArgumentNullException(nameof(sportRepository));
            _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<ActivityDto>> GetActivities(string userId, ActivitySearchDto searchDto)
        {
            try
            {
                var user = await _userRepository.Query()
                    .Include(u => u.Profile)
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var query = _activityRepository.Query()
                    .Include(a => a.Sport)
                    .Include(a => a.Location)
                    .Where(a => a.IsActive);

                // Apply sport filter
                if (searchDto.SportId.HasValue)
                {
                    query = query.Where(a => a.SportId == searchDto.SportId.Value);
                }

                // Apply difficulty filter
                if (!string.IsNullOrEmpty(searchDto.Difficulty))
                {
                    query = query.Where(a => a.DifficultyLevel == searchDto.Difficulty);
                }

                // Get all activities that match the filters
                var activities = await query.ToListAsync();

                // Filter and calculate distance if coordinates are provided
                var result = new List<ActivityDto>();
                foreach (var activity in activities)
                {
                    var activityDto = _mapper.Map<ActivityDto>(activity);

                    // Calculate distance if location is available
                    if (searchDto.Latitude.HasValue && searchDto.Longitude.HasValue && activity.Location?.Coordinates != null)
                    {
                        var userLocation = new Point(searchDto.Longitude.Value, searchDto.Latitude.Value) { SRID = 4326 };
                        var distance = userLocation.Distance(activity.Location.Coordinates) / 1000; // Convert to km
                        
                        // Apply distance filter
                        if (searchDto.Distance.HasValue && distance > searchDto.Distance.Value)
                        {
                            continue; // Skip activities that are too far
                        }
                        
                        activityDto.Distance = distance;
                    }
                    else if (user.Profile?.PreferredLocation != null && activity.Location?.Coordinates != null)
                    {
                        // Use user's preferred location if available
                        var distance = user.Profile.PreferredLocation.Distance(activity.Location.Coordinates) / 1000; // Convert to km
                        
                        // Apply distance filter
                        if (searchDto.Distance.HasValue && distance > searchDto.Distance.Value)
                        {
                            continue; // Skip activities that are too far
                        }
                        
                        activityDto.Distance = distance;
                    }

                    result.Add(activityDto);
                }

                // Sort by distance if available
                return result.OrderBy(a => a.Distance ?? double.MaxValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting activities for user {userId}");
                throw;
            }
        }

        public async Task<ActivityDto> GetActivityById(Guid activityId, string userId = null)
        {
            try
            {
                var activity = await _activityRepository.Query()
                    .Include(a => a.Sport)
                    .Include(a => a.Location)
                    .Include(a => a.CreatedBy)
                    .FirstOrDefaultAsync(a => a.ActivityId == activityId);

                if (activity == null)
                {
                    throw new KeyNotFoundException($"Activity not found with ID: {activityId}");
                }

                var activityDto = _mapper.Map<ActivityDto>(activity);

                // Calculate distance if user ID is provided
                if (!string.IsNullOrEmpty(userId) && activity.Location?.Coordinates != null)
                {
                    var user = await _userRepository.Query()
                        .Include(u => u.Profile)
                        .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                    if (user?.Profile?.PreferredLocation != null)
                    {
                        var distance = user.Profile.PreferredLocation.Distance(activity.Location.Coordinates) / 1000; // Convert to km
                        activityDto.Distance = distance;
                    }
                }

                return activityDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting activity with ID {activityId}");
                throw;
            }
        }

        public async Task<ActivityDto> CreateActivity(string userId, ActivityCreateDto activityDto)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                // Verify sport exists
                var sport = await _sportRepository.GetByIdAsync(activityDto.SportId);
                if (sport == null)
                {
                    throw new KeyNotFoundException($"Sport not found with ID: {activityDto.SportId}");
                }

                // Verify location exists if provided
                if (activityDto.LocationId.HasValue)
                {
                    var location = await _locationRepository.GetByIdAsync(activityDto.LocationId.Value);
                    if (location == null)
                    {
                        throw new KeyNotFoundException($"Location not found with ID: {activityDto.LocationId}");
                    }
                }

                var activity = new Activity
                {
                    ActivityId = Guid.NewGuid(),
                    SportId = activityDto.SportId,
                    LocationId = activityDto.LocationId,
                    Name = activityDto.Name,
                    Description = activityDto.Description,
                    RecurringSchedule = activityDto.RecurringSchedule,
                    DifficultyLevel = activityDto.DifficultyLevel,
                    MaxParticipants = activityDto.MaxParticipants,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = user.UserId
                };

                await _activityRepository.AddAsync(activity);
                await _activityRepository.SaveChangesAsync();

                return await GetActivityById(activity.ActivityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating activity by user {userId}");
                throw;
            }
        }

        public async Task<ActivityDto> UpdateActivity(Guid activityId, string userId, ActivityUpdateDto activityDto)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var activity = await _activityRepository.GetByIdAsync(activityId);
                if (activity == null)
                {
                    throw new KeyNotFoundException($"Activity not found with ID: {activityId}");
                }

                // Verify ownership or admin status
                if (activity.CreatedByUserId != user.UserId && !user.IsAdmin)
                {
                    throw new UnauthorizedAccessException("You are not authorized to update this activity");
                }

                // Verify sport exists if updating
                if (activityDto.SportId.HasValue)
                {
                    var sport = await _sportRepository.GetByIdAsync(activityDto.SportId.Value);
                    if (sport == null)
                    {
                        throw new KeyNotFoundException($"Sport not found with ID: {activityDto.SportId}");
                    }
                    activity.SportId = activityDto.SportId.Value;
                }

                // Verify location exists if updating
                if (activityDto.LocationId.HasValue)
                {
                    var location = await _locationRepository.GetByIdAsync(activityDto.LocationId.Value);
                    if (location == null)
                    {
                        throw new KeyNotFoundException($"Location not found with ID: {activityDto.LocationId}");
                    }
                    activity.LocationId = activityDto.LocationId;
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(activityDto.Name))
                {
                    activity.Name = activityDto.Name;
                }

                if (activityDto.Description != null) // Allow clearing description
                {
                    activity.Description = activityDto.Description;
                }

                if (activityDto.RecurringSchedule != null) // Allow clearing schedule
                {
                    activity.RecurringSchedule = activityDto.RecurringSchedule;
                }

                if (!string.IsNullOrEmpty(activityDto.DifficultyLevel))
                {
                    activity.DifficultyLevel = activityDto.DifficultyLevel;
                }

                if (activityDto.MaxParticipants.HasValue)
                {
                    activity.MaxParticipants = activityDto.MaxParticipants;
                }

                if (activityDto.IsActive.HasValue)
                {
                    activity.IsActive = activityDto.IsActive.Value;
                }

                await _activityRepository.UpdateAsync(activity);
                await _activityRepository.SaveChangesAsync();

                return await GetActivityById(activity.ActivityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating activity {activityId} by user {userId}");
                throw;
            }
        }

        public async Task DeleteActivity(Guid activityId, string userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var activity = await _activityRepository.GetByIdAsync(activityId);
                if (activity == null)
                {
                    throw new KeyNotFoundException($"Activity not found with ID: {activityId}");
                }

                // Verify ownership or admin status
                if (activity.CreatedByUserId != user.UserId && !user.IsAdmin)
                {
                    throw new UnauthorizedAccessException("You are not authorized to delete this activity");
                }

                // Soft delete by marking inactive
                activity.IsActive = false;
                await _activityRepository.UpdateAsync(activity);
                await _activityRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting activity {activityId} by user {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<ActivityDto>> GetUserActivities(string userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                var activities = await _activityRepository.Query()
                    .Include(a => a.Sport)
                    .Include(a => a.Location)
                    .Where(a => a.CreatedByUserId == user.UserId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ActivityDto>>(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting activities created by user {userId}");
                throw;
            }
        }

        public async Task<PaginatedResponse<ActivityDto>> GetNearbyActivities(double latitude, double longitude, double distance = 10, int page = 1, int pageSize = 10)
        {
            try
            {
                // Create a point from the provided coordinates
                var userLocation = new Point(longitude, latitude) { SRID = 4326 };
                
                // Get locations within the distance
                var nearbyLocations = await _locationRepository.Query()
                    .Where(l => l.Coordinates.Distance(userLocation) <= distance * 1000) // Convert km to meters
                    .Select(l => l.LocationId)
                    .ToListAsync();
                
                // Get activities at these locations
                var query = _activityRepository.Query()
                    .Include(a => a.Sport)
                    .Include(a => a.Location)
                    .Where(a => a.IsActive && a.LocationId.HasValue && nearbyLocations.Contains(a.LocationId.Value))
                    .OrderBy(a => a.Location.Coordinates.Distance(userLocation));
                
                // Count total results
                var totalItems = await query.CountAsync();
                
                // Get paginated results
                var activities = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                // Map to DTOs with distance
                var activityDtos = new List<ActivityDto>();
                foreach (var activity in activities)
                {
                    var dto = _mapper.Map<ActivityDto>(activity);
                    
                    if (activity.Location?.Coordinates != null)
                    {
                        dto.Distance = userLocation.Distance(activity.Location.Coordinates) / 1000; // Convert to km
                    }
                    
                    activityDtos.Add(dto);
                }
                
                return new PaginatedResponse<ActivityDto>
                {
                    Success = true,
                    Items = activityDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting nearby activities at coordinates ({latitude}, {longitude})");
                throw;
            }
        }

        public async Task<IEnumerable<ActivityDto>> GetRecommendedActivities(string userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .Include(u => u.Profile)
                    .Include(u => u.Sports)
                        .ThenInclude(us => us.Sport)
                    .FirstOrDefaultAsync(u => u.Auth0Id == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                // Get user's sports
                var userSportIds = user.Sports.Select(us => us.SportId).ToList();
                if (!userSportIds.Any())
                {
                    return new List<ActivityDto>();
                }

                // Get user's location
                Point userLocation = user.Profile?.PreferredLocation;
                int maxDistance = user.Profile?.MaxTravelDistance ?? 50; // Default to 50km

                // Get activities matching user's sports
                var query = _activityRepository.Query()
                    .Include(a => a.Sport)
                    .Include(a => a.Location)
                    .Where(a => a.IsActive && userSportIds.Contains(a.SportId))
                    .OrderByDescending(a => a.CreatedAt);

                var activities = await query.ToListAsync();

                // Filter by distance and add distance property
                var recommendedActivities = new List<ActivityDto>();
                foreach (var activity in activities)
                {
                    var activityDto = _mapper.Map<ActivityDto>(activity);

                    if (userLocation != null && activity.Location?.Coordinates != null)
                    {
                        var distance = userLocation.Distance(activity.Location.Coordinates) / 1000; // Convert to km
                        
                        // Skip if too far
                        if (distance > maxDistance)
                        {
                            continue;
                        }
                        
                        activityDto.Distance = distance;
                    }

                    recommendedActivities.Add(activityDto);
                }

                // Sort by relevance (distance, matching skill level, etc.)
                return recommendedActivities
                    .OrderBy(a => a.Distance ?? double.MaxValue) // Sort by distance if available
                    .Take(20); // Limit to 20 recommendations
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting recommended activities for user {userId}");
                throw;
            }
        }
    }
}