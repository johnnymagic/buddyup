
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
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserProfile> _profileRepository;
        private readonly IRepository<UserSport> _userSportRepository;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;

        public UserService(
            IRepository<User> userRepository,
            IRepository<UserProfile> profileRepository,
            IRepository<UserSport> userSportRepository,
            ILogger<UserService> logger,
            IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
            _userSportRepository = userSportRepository ?? throw new ArgumentNullException(nameof(userSportRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<UserDto> GetUserByAuth0Id(string auth0Id)
        {
            try
            {
                var user = await _userRepository.Query()
                    .Include(u => u.Profile)
                    .Include(u => u.Sports)
                        .ThenInclude(us => us.Sport)
                    .FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);

                if (user == null)
                {
                    _logger.LogInformation($"User not found with Auth0 ID: {auth0Id}");
                    return null;
                }

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user with Auth0 ID: {auth0Id}");
                throw;
            }
        }

        public async Task<UserDto> GetUserById(Guid userId)
        {
            try
            {
                var user = await _userRepository.Query()
                    .Include(u => u.Profile)
                    .Include(u => u.Sports)
                        .ThenInclude(us => us.Sport)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    _logger.LogInformation($"User not found with ID: {userId}");
                    return null;
                }

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user with ID: {userId}");
                throw;
            }
        }

        public async Task<UserDto> CreateUser(UserDto userDto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == userDto.Auth0Id || u.Email == userDto.Email);

                if (existingUser != null)
                {
                    throw new InvalidOperationException($"User already exists with Auth0 ID: {userDto.Auth0Id} or Email: {userDto.Email}");
                }

                var user = _mapper.Map<User>(userDto);
                
                // Set default values
                user.UserId = Guid.NewGuid();
                user.CreatedAt = DateTime.UtcNow;
                user.LastLoginAt = DateTime.UtcNow;
                user.Active = true;
                user.IsAdmin = false; // Default to non-admin
                user.IsVerified = false; // Default to unverified

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating user: {userDto.Email}");
                throw;
            }
        }

        public async Task<UserDto> UpdateUser(UserDto userDto)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.UserId == userDto.UserId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userDto.UserId}");
                }

                // Update only basic properties (not status, admin rights, etc.)
                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                
                // Only update email if it's different and not already taken
                if (user.Email != userDto.Email)
                {
                    var existingUser = await _userRepository.Query()
                        .FirstOrDefaultAsync(u => u.Email == userDto.Email && u.UserId != user.UserId);
                    
                    if (existingUser != null)
                    {
                        throw new InvalidOperationException($"Email is already in use: {userDto.Email}");
                    }
                    
                    user.Email = userDto.Email;
                }

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user: {userDto.UserId}");
                throw;
            }
        }

        public async Task UpdateLastLogin(string auth0Id)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);

                if (user == null)
                {
                    _logger.LogWarning($"User not found for login update: {auth0Id}");
                    return;
                }

                user.LastLoginAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating last login for user: {auth0Id}");
                throw;
            }
        }

        public async Task<UserDto> UpdateUserStatus(Guid userId, bool active)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                user.Active = active;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for user: {userId}");
                throw;
            }
        }

        public async Task<PaginatedResponse<UserListItemDto>> GetUsers(UserSearchDto searchDto)
        {
            try
            {
                var query = _userRepository.Query()
                    .Include(u => u.Profile)
                    .Include(u => u.Sports)
                        .ThenInclude(us => us.Sport)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchDto.Search))
                {
                    query = query.Where(u => 
                        u.FirstName.Contains(searchDto.Search) || 
                        u.LastName.Contains(searchDto.Search) || 
                        u.Email.Contains(searchDto.Search));
                }

                if (!string.IsNullOrWhiteSpace(searchDto.Status))
                {
                    bool isActive = searchDto.Status.ToLower() == "active";
                    query = query.Where(u => u.Active == isActive);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.IsVerified))
                {
                    bool isVerified = searchDto.IsVerified.ToLower() == "true";
                    query = query.Where(u => u.IsVerified == isVerified);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.Sport))
                {
                    // Try to parse as GUID first
                    if (Guid.TryParse(searchDto.Sport, out Guid sportId))
                    {
                        query = query.Where(u => u.Sports.Any(us => us.SportId == sportId));
                    }
                    else
                    {
                        // Otherwise search by name
                        query = query.Where(u => u.Sports.Any(us => us.Sport.Name.Contains(searchDto.Sport)));
                    }
                }

                // Count total items
                var totalItems = await query.CountAsync();

                // Apply pagination
                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var userDtos = _mapper.Map<List<UserListItemDto>>(users);

                return new PaginatedResponse<UserListItemDto>
                {
                    Success = true,
                    Items = userDtos,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)searchDto.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                throw;
            }
        }

        public async Task<UserDto> SetAdminStatus(Guid userId, bool isAdmin)
        {
            try
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                user.IsAdmin = isAdmin;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting admin status for user: {userId}");
                throw;
            }
        }

        public async Task<UserDto> SetVerificationStatus(Guid userId, bool isVerified)
        {
            try
            {
                var user = await _userRepository.Query()
                    .Include(u => u.Profile)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with ID: {userId}");
                }

                user.IsVerified = isVerified;
                
                // Also update profile verification status
                if (user.Profile != null)
                {
                    user.Profile.VerificationStatus = isVerified ? "Verified" : "Unverified";
                    user.Profile.VerificationCompletedAt = isVerified ? DateTime.UtcNow : null;
                    user.Profile.UpdatedAt = DateTime.UtcNow;
                }

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting verification status for user: {userId}");
                throw;
            }
        }
    }
}