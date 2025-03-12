import React, { useState, useEffect } from 'react';
import { useAuth } from '../../hooks/useAuth';
import { api } from '../../services/ApiService';
import Button from '../common/Button';
import Input from '../common/Input';

/**
 * ProfileForm component for editing user profile information
 * This includes personal details, preferences, and sports/skill levels
 */

const ProfileForm = () => {
  const { userProfile, updateProfile } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [sports, setSports] = useState([]);
  const [userSports, setUserSports] = useState([]);
  
  // Form state
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    bio: '',
    profilePictureUrl: '',
    maxTravelDistance: 20,
    preferredDays: [],
    preferredTimes: []
  });

  // Load user profile and sports data
  useEffect(() => {
    if (userProfile) {
      setFormData({
        firstName: userProfile.firstName || '',
        lastName: userProfile.lastName || '',
        bio: userProfile.bio || '',
        profilePictureUrl: userProfile.profilePictureUrl || '',
        maxTravelDistance: userProfile.maxTravelDistance || 20,
        preferredDays: userProfile.preferredDays || [],
        preferredTimes: userProfile.preferredTimes || []
      });
    }
    
    const fetchSports = async () => {
      try {
        const sportsData = await api.get('/api/sports');
        setSports(sportsData);
      } catch (error) {
        console.error('Error fetching sports', error);
        setErrorMessage('Could not load sports list');
      }
    };
    
    const fetchUserSports = async () => {
      if (!userProfile) return;
      
      try {
        const userSportsData = await api.get('/api/profile/sports');
        setUserSports(userSportsData);
      } catch (error) {
        console.error('Error fetching user sports', error);
      }
    };
    
    fetchSports();
    fetchUserSports();
  }, [userProfile]);

  // Handle form input changes
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  // Handle checkbox changes for days of week
  const handleDayChange = (day) => {
    setFormData(prev => {
      const currentDays = [...prev.preferredDays];
      
      if (currentDays.includes(day)) {
        return {
          ...prev,
          preferredDays: currentDays.filter(d => d !== day)
        };
      } else {
        return {
          ...prev,
          preferredDays: [...currentDays, day]
        };
      }
    });
  };
  
  // Handle checkbox changes for preferred times
  const handleTimeChange = (time) => {
    setFormData(prev => {
      const currentTimes = [...prev.preferredTimes];
      
      if (currentTimes.includes(time)) {
        return {
          ...prev,
          preferredTimes: currentTimes.filter(t => t !== time)
        };
      } else {
        return {
          ...prev,
          preferredTimes: [...currentTimes, time]
        };
      }
    });
  };
  
  // Handle sport skill level changes
  const handleSportSkillChange = async (sportId, skillLevel) => {
    try {
      setIsLoading(true);
      
      const existingSport = userSports.find(us => us.sportId === sportId);
      
      if (existingSport) {
        // Update existing user-sport
        await api.put(`/api/profile/sports/${existingSport.userSportId}`, {
          skillLevel
        });
      } else {
        // Add new user-sport
        await api.post('/api/profile/sports', {
          sportId,
          skillLevel
        });
      }
      
      // Refresh user sports
      const updatedUserSports = await api.get('/api/profile/sports');
      setUserSports(updatedUserSports);
      
      setSuccessMessage('Sport preferences updated successfully');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (error) {
      console.error('Error updating sport preferences', error);
      setErrorMessage('Failed to update sport preferences');
      setTimeout(() => setErrorMessage(''), 3000);
    } finally {
      setIsLoading(false);
    }
  };
  
  // Handle sport removal
  const handleRemoveSport = async (userSportId) => {
    try {
      setIsLoading(true);
      await api.delete(`/api/profile/sports/${userSportId}`);
      
      // Refresh user sports
      const updatedUserSports = await api.get('/api/profile/sports');
      setUserSports(updatedUserSports);
      
      setSuccessMessage('Sport removed successfully');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (error) {
      console.error('Error removing sport', error);
      setErrorMessage('Failed to remove sport');
      setTimeout(() => setErrorMessage(''), 3000);
    } finally {
      setIsLoading(false);
    }
  };
  
  // Handle profile form submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setErrorMessage('');
    setSuccessMessage('');
    
    try {
      await updateProfile({
        firstName: formData.firstName,
        lastName: formData.lastName,
        bio: formData.bio,
        profilePictureUrl: formData.profilePictureUrl,
        maxTravelDistance: parseInt(formData.maxTravelDistance, 10),
        preferredDays: formData.preferredDays,
        preferredTimes: formData.preferredTimes
      });
      
      setSuccessMessage('Profile updated successfully');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (error) {
      console.error('Error updating profile', error);
      setErrorMessage('Failed to update profile');
      setTimeout(() => setErrorMessage(''), 3000);
    } finally {
      setIsLoading(false);
    }
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
    { id: 'beginner', label: 'Beginner' },
    { id: 'intermediate', label: 'Intermediate' },
    { id: 'advanced', label: 'Advanced' },
    { id: 'expert', label: 'Expert' }
  ];
  
  return (
    <div className="bg-white rounded-lg shadow-md p-6 max-w-3xl mx-auto">
      <h2 className="text-2xl font-bold mb-6">Edit Profile</h2>
      
      {/* Error/Success messages */}
      {errorMessage && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          {errorMessage}
        </div>
      )}
      {successMessage && (
        <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-4">
          {successMessage}
        </div>
      )}
      
      <form onSubmit={handleSubmit}>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
          <div>
            <label className="block text-gray-700 font-medium mb-2" htmlFor="firstName">
              First Name
            </label>
            <Input
              id="firstName"
              name="firstName"
              type="text"
              value={formData.firstName}
              onChange={handleInputChange}
              required
            />
          </div>
          
          <div>
            <label className="block text-gray-700 font-medium mb-2" htmlFor="lastName">
              Last Name
            </label>
            <Input
              id="lastName"
              name="lastName"
              type="text"
              value={formData.lastName}
              onChange={handleInputChange}
              required
            />
          </div>
        </div>
        
        <div className="mb-6">
          <label className="block text-gray-700 font-medium mb-2" htmlFor="bio">
            Bio
          </label>
          <textarea
            id="bio"
            name="bio"
            value={formData.bio}
            onChange={handleInputChange}
            rows="4"
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          ></textarea>
          <p className="text-sm text-gray-500 mt-1">
            Tell potential workout buddies about yourself, your fitness journey, and what you're looking for.
          </p>
        </div>
        
        <div className="mb-6">
          <label className="block text-gray-700 font-medium mb-2" htmlFor="profilePictureUrl">
            Profile Picture URL
          </label>
          <Input
            id="profilePictureUrl"
            name="profilePictureUrl"
            type="url"
            value={formData.profilePictureUrl}
            onChange={handleInputChange}
            placeholder="https://example.com/your-photo.jpg"
          />
        </div>
        
        <div className="mb-6">
          <label className="block text-gray-700 font-medium mb-2" htmlFor="maxTravelDistance">
            Maximum Travel Distance (km): {formData.maxTravelDistance}
          </label>
          <input
            id="maxTravelDistance"
            name="maxTravelDistance"
            type="range"
            min="1"
            max="100"
            value={formData.maxTravelDistance}
            onChange={handleInputChange}
            className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
          />
          <div className="flex justify-between text-xs text-gray-500">
            <span>1km</span>
            <span>50km</span>
            <span>100km</span>
          </div>
        </div>
        
        <div className="mb-6">
          <label className="block text-gray-700 font-medium mb-2">
            Available Days
          </label>
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
            {daysOfWeek.map(day => (
              <div key={day.id} className="flex items-center">
                <input
                  id={`day-${day.id}`}
                  type="checkbox"
                  checked={formData.preferredDays.includes(day.id)}
                  onChange={() => handleDayChange(day.id)}
                  className="mr-2 h-4 w-4"
                />
                <label htmlFor={`day-${day.id}`}>{day.label}</label>
              </div>
            ))}
          </div>
        </div>
        
        <div className="mb-8">
          <label className="block text-gray-700 font-medium mb-2">
            Preferred Times
          </label>
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
            {timesOfDay.map(time => (
              <div key={time.id} className="flex items-center">
                <input
                  id={`time-${time.id}`}
                  type="checkbox"
                  checked={formData.preferredTimes.includes(time.id)}
                  onChange={() => handleTimeChange(time.id)}
                  className="mr-2 h-4 w-4"
                />
                <label htmlFor={`time-${time.id}`}>{time.label}</label>
              </div>
            ))}
          </div>
        </div>
        
        <div className="border-t border-gray-200 pt-6 mb-6">
          <h3 className="text-xl font-semibold mb-4">Your Sports</h3>
          
          {userSports.length === 0 ? (
            <p className="text-gray-500 italic">You haven't added any sports yet.</p>
          ) : (
            <div className="space-y-4">
              {userSports.map(userSport => {
                const sport = sports.find(s => s.sportId === userSport.sportId);
                return (
                  <div key={userSport.userSportId} className="flex flex-wrap items-center justify-between p-3 border border-gray-200 rounded-md">
                    <div className="flex items-center mr-4">
                      {sport?.iconUrl && (
                        <img src={sport.iconUrl} alt={sport.name} className="w-8 h-8 mr-2" />
                      )}
                      <span className="font-medium">{sport?.name}</span>
                    </div>
                    
                    <div className="flex items-center flex-wrap mt-2 sm:mt-0">
                      <select
                        value={userSport.skillLevel}
                        onChange={(e) => handleSportSkillChange(userSport.sportId, e.target.value)}
                        className="mr-2 p-2 border border-gray-300 rounded"
                      >
                        {skillLevels.map(level => (
                          <option key={level.id} value={level.id}>
                            {level.label}
                          </option>
                        ))}
                      </select>
                      
                      <button
                        type="button"
                        onClick={() => handleRemoveSport(userSport.userSportId)}
                        className="text-red-600 hover:text-red-800"
                      >
                        Remove
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
          
          <div className="mt-4">
            <h4 className="text-lg font-medium mb-2">Add a Sport</h4>
            <div className="flex flex-wrap gap-2">
              {sports
                .filter(sport => !userSports.some(us => us.sportId === sport.sportId))
                .map(sport => (
                  <button
                    key={sport.sportId}
                    type="button"
                    onClick={() => handleSportSkillChange(sport.sportId, 'beginner')}
                    className="px-3 py-1 bg-blue-100 text-blue-800 rounded-full hover:bg-blue-200"
                  >
                    {sport.name}
                  </button>
                ))}
            </div>
          </div>
        </div>
        
        <div className="flex justify-end">
          <Button
            type="submit"
            disabled={isLoading}
            className="bg-blue-600 hover:bg-blue-700 text-white"
          >
            {isLoading ? 'Saving...' : 'Save Profile'}
          </Button>
        </div>
      </form>
    </div>
  );
};

export default ProfileForm;