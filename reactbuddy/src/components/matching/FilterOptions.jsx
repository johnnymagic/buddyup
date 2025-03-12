import React, { useState, useEffect } from 'react';
import { api } from '../../services/ApiService';

const FilterOptions = ({ filters, updateFilters }) => {
  const [sports, setSports] = useState([]);
  const [loading, setLoading] = useState(true);
  
  // Fetch available sports
  useEffect(() => {
    const fetchSports = async () => {
      try {
        setLoading(true);
        const response = await api.get('/api/sports');
        setSports(response);
      } catch (error) {
        console.error('Error fetching sports:', error);
      } finally {
        setLoading(false);
      }
    };
    
    fetchSports();
  }, []);
  
  // Handle filter changes
  const handleSportChange = (e) => {
    updateFilters({ sport: e.target.value });
  };
  
  const handleSkillLevelChange = (e) => {
    updateFilters({ skillLevel: e.target.value });
  };
  
  const handleDistanceChange = (e) => {
    updateFilters({ distance: parseInt(e.target.value, 10) });
  };
  
  const handleDayChange = (day) => {
    const currentDays = [...filters.days];
    
    if (currentDays.includes(day)) {
      updateFilters({ days: currentDays.filter(d => d !== day) });
    } else {
      updateFilters({ days: [...currentDays, day] });
    }
  };
  
  const handleTimeChange = (time) => {
    const currentTimes = [...filters.times];
    
    if (currentTimes.includes(time)) {
      updateFilters({ times: currentTimes.filter(t => t !== time) });
    } else {
      updateFilters({ times: [...currentTimes, time] });
    }
  };
  
  // Reset all filters
  const handleReset = () => {
    updateFilters({
      sport: '',
      skillLevel: '',
      distance: 50,
      days: [],
      times: []
    });
  };
  
  // Days of the week options
  const daysOfWeek = [
    { id: 'monday', label: 'Monday' },
    { id: 'tuesday', label: 'Tuesday' },
    { id: 'wednesday', label: 'Wednesday' },
    { id: 'thursday', label: 'Thursday' },
    { id: 'friday', label: 'Friday' },
    { id: 'saturday', label: 'Saturday' },
    { id: 'sunday', label: 'Sunday' }
  ];
  
  // Time of day options
  const timesOfDay = [
    { id: 'morning', label: 'Morning' },
    { id: 'afternoon', label: 'Afternoon' },
    { id: 'evening', label: 'Evening' },
    { id: 'night', label: 'Night' }
  ];
  
  // Skill level options
  const skillLevels = [
    { id: '', label: 'Any Skill Level' },
    { id: 'beginner', label: 'Beginner' },
    { id: 'intermediate', label: 'Intermediate' },
    { id: 'advanced', label: 'Advanced' },
    { id: 'expert', label: 'Expert' }
  ];
  
  return (
    <div className="bg-gray-50 p-4 rounded-lg mb-6">
      <h3 className="text-lg font-semibold mb-4">Filter Workout Buddies</h3>
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Sport
          </label>
          <select
            value={filters.sport}
            onChange={handleSportChange}
            className="w-full p-2 border border-gray-300 rounded-md"
            disabled={loading}
          >
            <option value="">Any Sport</option>
            {sports.map(sport => (
              <option key={sport.sportId} value={sport.sportId}>
                {sport.name}
              </option>
            ))}
          </select>
        </div>
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Skill Level
          </label>
          <select
            value={filters.skillLevel}
            onChange={handleSkillLevelChange}
            className="w-full p-2 border border-gray-300 rounded-md"
          >
            {skillLevels.map(level => (
              <option key={level.id} value={level.id}>
                {level.label}
              </option>
            ))}
          </select>
        </div>
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Maximum Distance: {filters.distance} km
          </label>
          <input
            type="range"
            min="1"
            max="100"
            value={filters.distance}
            onChange={handleDistanceChange}
            className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
          />
          <div className="flex justify-between text-xs text-gray-500">
            <span>1km</span>
            <span>50km</span>
            <span>100km</span>
          </div>
        </div>
      </div>
      
      <div className="mt-4 grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Available Days
          </label>
          <div className="flex flex-wrap gap-2">
            {daysOfWeek.map(day => (
              <button
                key={day.id}
                type="button"
                className={`px-3 py-1 text-sm rounded-full ${
                  filters.days.includes(day.id)
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                }`}
                onClick={() => handleDayChange(day.id)}
              >
                {day.label}
              </button>
            ))}
          </div>
        </div>
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Preferred Times
          </label>
          <div className="flex flex-wrap gap-2">
            {timesOfDay.map(time => (
              <button
                key={time.id}
                type="button"
                className={`px-3 py-1 text-sm rounded-full ${
                  filters.times.includes(time.id)
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                }`}
                onClick={() => handleTimeChange(time.id)}
              >
                {time.label}
              </button>
            ))}
          </div>
        </div>
      </div>
      
      <div className="mt-4 flex justify-end">
        <button
          type="button"
          onClick={handleReset}
          className="px-4 py-2 text-sm text-gray-700 hover:text-gray-900"
        >
          Reset Filters
        </button>
      </div>
    </div>
  );
};

export default FilterOptions;