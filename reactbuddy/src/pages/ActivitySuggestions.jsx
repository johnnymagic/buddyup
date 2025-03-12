import React, { useState, useEffect } from 'react';
import { api } from '../services/ApiService';
import { useProfile } from '../hooks/useProfile';
import Spinner from '../components/common/Spinner';

/**
 * Activity Suggestions page component
 */
const ActivitySuggestions = () => {
  const { userSports } = useProfile();
  
  const [activities, setActivities] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filter, setFilter] = useState({
    sport: '',
    distance: 25, // km
    difficulty: ''
  });

  // Fetch activities when component mounts or filter changes
  useEffect(() => {
    const fetchActivities = async () => {
      setLoading(true);
      setError(null);
      
      try {
        const queryParams = new URLSearchParams();
        if (filter.sport) queryParams.append('sportId', filter.sport);
        if (filter.distance) queryParams.append('distance', filter.distance);
        if (filter.difficulty) queryParams.append('difficulty', filter.difficulty);
        
        const response = await api.get(`/api/activities?${queryParams.toString()}`);
        setActivities(response);
      } catch (err) {
        console.error('Error fetching activities', err);
        setError('Failed to load activities');
      } finally {
        setLoading(false);
      }
    };
    
    fetchActivities();
  }, [filter]);

  // Handle filter changes
  const handleFilterChange = (e) => {
    const { name, value } = e.target;
    setFilter(prev => ({
      ...prev,
      [name]: value
    }));
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-6">Activity Suggestions</h1>
      
      {/* Filters */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-8">
        <h2 className="text-xl font-semibold mb-4">Find Activities</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="sport">
              Sport
            </label>
            <select
              id="sport"
              name="sport"
              value={filter.sport}
              onChange={handleFilterChange}
              className="w-full p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">All Sports</option>
              {userSports.map(userSport => (
                <option key={userSport.sportId} value={userSport.sportId}>
                  {userSport.sportName}
                </option>
              ))}
            </select>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="distance">
              Maximum Distance: {filter.distance} km
            </label>
            <input
              id="distance"
              name="distance"
              type="range"
              min="1"
              max="100"
              value={filter.distance}
              onChange={handleFilterChange}
              className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="difficulty">
              Difficulty Level
            </label>
            <select
              id="difficulty"
              name="difficulty"
              value={filter.difficulty}
              onChange={handleFilterChange}
              className="w-full p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Any Difficulty</option>
              <option value="beginner">Beginner</option>
              <option value="intermediate">Intermediate</option>
              <option value="advanced">Advanced</option>
              <option value="expert">Expert</option>
            </select>
          </div>
        </div>
      </div>
      
      {/* Activity List */}
      {loading ? (
        <div className="flex justify-center py-12">
          <Spinner size="lg" />
        </div>
      ) : error ? (
        <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-6">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg className="h-5 w-5 text-red-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm text-red-700">{error}</p>
            </div>
          </div>
        </div>
      ) : activities.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-lg shadow-md">
          <svg
            className="mx-auto h-12 w-12 text-gray-400"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z"
            ></path>
          </svg>
          <h3 className="mt-2 text-lg font-medium text-gray-900">No activities found</h3>
          <p className="mt-1 text-gray-500">
            Try adjusting your filters to see more activities.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {activities.map(activity => (
            <div key={activity.activityId} className="bg-white rounded-lg shadow-md overflow-hidden">
              <div className="p-6">
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="text-xl font-semibold text-gray-900">{activity.name}</h3>
                    <p className="text-sm text-gray-500">
                      {activity.location?.name || 'Location not specified'}
                    </p>
                  </div>
                  <span className={`px-2 py-1 text-xs rounded-full ${
                    activity.difficultyLevel === 'beginner' ? 'bg-green-100 text-green-800' :
                    activity.difficultyLevel === 'intermediate' ? 'bg-blue-100 text-blue-800' :
                    activity.difficultyLevel === 'advanced' ? 'bg-orange-100 text-orange-800' :
                    'bg-red-100 text-red-800'
                  }`}>
                    {activity.difficultyLevel}
                  </span>
                </div>
                
                <div className="bg-blue-50 rounded-md p-3 mb-4">
                  <div className="flex items-center">
                    <img 
                      src={activity.sport?.iconUrl} 
                      alt={activity.sport?.name} 
                      className="w-6 h-6 mr-2"
                      onError={(e) => {
                        e.target.onerror = null;
                        e.target.style.display = 'none';
                      }}
                    />
                    <span>{activity.sport?.name}</span>
                  </div>
                </div>
                
                <p className="text-gray-600 mb-4">{activity.description}</p>
                
                {activity.recurringSchedule && (
                  <div className="mb-4">
                    <h4 className="font-medium text-gray-700 mb-1">Schedule</h4>
                    <p className="text-sm text-gray-600">{activity.recurringSchedule}</p>
                  </div>
                )}
                
                {activity.maxParticipants && (
                  <div className="mb-4">
                    <h4 className="font-medium text-gray-700 mb-1">Group Size</h4>
                    <p className="text-sm text-gray-600">Up to {activity.maxParticipants} participants</p>
                  </div>
                )}
                
                <div className="flex justify-between items-center pt-4 border-t border-gray-100">
                  <span className="text-sm text-gray-500">
                    {activity.distance ? `${activity.distance.toFixed(1)} km away` : 'Distance unknown'}
                  </span>
                  <button
                    className="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded hover:bg-blue-700"
                  >
                    Get Details
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default ActivitySuggestions;