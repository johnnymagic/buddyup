
using AutoMapper;
using BuddyUp.API.Data.Repositories;
using BuddyUp.API.Models.Domain;
using BuddyUp.API.Data.Repositories;
using BuddyUp.API.Models.DTOs;
using BuddyUp.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;




namespace BuddyUp.API.Services.Implementations
{


    public class SportService : ISportService
    {
     
        private readonly IRepository<Sport> _sportRepository;
        private readonly ILogger<ProfileService> _logger;
        private readonly IMapper _mapper;

        public SportService(
           
            IRepository<Sport> sportRepository,
            ILogger<ProfileService> logger,
            IMapper mapper)
        {
            _sportRepository = sportRepository ?? throw new ArgumentNullException(nameof(sportRepository));
             _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SportDto> GetSportById(Guid sportId)
        {
            try
            {
                var sport = await _sportRepository.Query()
                .FirstOrDefaultAsync(v=> v.SportId == sportId);

                if (sport == null)
                {
                    _logger.LogInformation($"Sport not found for sport {sportId}");
                    return null;
                }

                return _mapper.Map<SportDto>(sport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving profile for user {sportId}");
                throw;
            }
        }

    }
}