import React, { useEffect, useState } from 'react';
import { useActivity } from '../context/ActivityContext';
import { useProfile } from '../context/ProfileContext';
import { Link } from 'react-router-dom';

const ActivitySuggestions = () => {
  const { 
    activities, 
    loading, 
    error, 
    filter, 
    updateFilter, 
    fetchActivities,
    fetchRecommendedActivities,
    recommendedActivities
  } = useActivity();
  
  const { availableSports, profileData } = useProfile();
  
  const [activeTab, setActiveTab] = useState('all'); // 'all' or 'recommended'

  // Fetch data on component mount
  useEffect(() => {
    fetchActivities();
    fetchRecommendedActivities();
  }, []);

  // Handle filter changes
  const handleFilterChange = (e) => {
    const { name, value } = e.target;
    
    // Log the filter change for debugging
    console.log(`Filter changed: ${name} = ${value}`);
    
    updateFilter({ [name]: value });
  };

  const getActivityStatusBadge = (activity) => {
    return (
      <span className={`px-2 py-1 text-xs rounded-full ${
        activity.difficultyLevel === 'beginner' ? 'bg-green-100 text-green-800' :
        activity.difficultyLevel === 'intermediate' ? 'bg-blue-100 text-blue-800' :
        activity.difficultyLevel === 'advanced' ? 'bg-orange-100 text-orange-800' :
        'bg-red-100 text-red-800'
      }`}>
        {activity.difficultyLevel}
      </span>
    );
  };

  // Render activity card
  const renderActivityCard = (activity) => (
    <div key={activity.activityId} className="bg-white rounded-lg shadow-md overflow-hidden">
      <div className="p-6">
        <div className="flex justify-between items-start mb-4">
          <div>
            <h3 className="text-xl font-semibold text-gray-900">{activity.name}</h3>
            <p className="text-sm text-gray-500">
              {activity.location?.name || 'Location not specified'}
            </p>
          </div>
          {getActivityStatusBadge(activity)}
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
          <Link
            to={`/activity/${activity.activityId}`}
            className="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded hover:bg-blue-700 transition-colors"
          >
            View Details
          </Link>
        </div>
      </div>
    </div>
  );

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-6">Activity Suggestions</h1>
      
      {/* Tabs */}
      <div className="flex border-b border-gray-200 mb-6">
        <button
          className={`py-3 px-6 font-medium ${
            activeTab === 'all' 
              ? 'text-blue-600 border-b-2 border-blue-600' 
              : 'text-gray-500 hover:text-gray-700'
          }`}
          onClick={() => setActiveTab('all')}
        >
          All Activities
        </button>
        <button
          className={`py-3 px-6 font-medium ${
            activeTab === 'recommended' 
              ? 'text-blue-600 border-b-2 border-blue-600' 
              : 'text-gray-500 hover:text-gray-700'
          }`}
          onClick={() => setActiveTab('recommended')}
        >
          Recommended For You
        </button>
      </div>
      
      {/* Filters - Only show on "All Activities" tab */}
      {activeTab === 'all' && (
        <div className="bg-white rounded-lg shadow-md p-6 mb-8">
          <h2 className="text-xl font-semibold mb-4">Find Activities</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="sportId">
                Sport
              </label>
              <select
                id="sportId"
                name="sportId"
                value={filter.sportId}
                onChange={handleFilterChange}
                className="w-full p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">All Sports</option>
                {Array.isArray(availableSports) && availableSports.length > 0 ? (
                  availableSports.map(sport => (
                    <option key={sport.sportId} value={sport.sportId}>
                      {sport.name}
                    </option>
                  ))
                ) : (
                  <option value="" disabled>Loading sports...</option>
                )}
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
                <option value="all">Any Difficulty</option>
                <option value="beginner">Beginner</option>
                <option value="intermediate">Intermediate</option>
                <option value="advanced">Advanced</option>
                <option value="expert">Expert</option>
              </select>
            </div>
          </div>
        </div>
      )}
      
      {/* Loading State */}
      {loading && (
        <div className="flex justify-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
        </div>
      )}
      
      {/* Error State */}
      {error && !loading && (
        <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-6">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg className="h-5 w-5 text-red-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm text-red-700">{error}</p>
            </div>
          </div>
        </div>
      )}
      
      {/* Activity Lists */}
      {!loading && !error && (
        <>
          {/* All Activities Tab */}
          {activeTab === 'all' && (
            <>
              {activities.length === 0 ? (
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
                  {activities.map(activity => renderActivityCard(activity))}
                </div>
              )}
            </>
          )}
          
          {/* Recommended Activities Tab */}
          {activeTab === 'recommended' && (
            <>
              {recommendedActivities.length === 0 ? (
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
                      d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"
                    ></path>
                  </svg>
                  <h3 className="mt-2 text-lg font-medium text-gray-900">No recommendations yet</h3>
                  <p className="mt-1 text-gray-500">
                    Complete your profile and add sports preferences to get personalized recommendations.
                  </p>
                  <div className="mt-4">
                    <Link
                      to="/profile"
                      className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                    >
                      Update Profile
                    </Link>
                  </div>
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                  {recommendedActivities.map(activity => renderActivityCard(activity))}
                </div>
              )}
            </>
          )}
        </>
      )}
      
      {/* Create Activity Button */}
      <div className="fixed bottom-6 right-6">
        <Link
          to="/activities/create"
          className="flex items-center justify-center h-14 w-14 rounded-full bg-blue-600 text-white shadow-lg hover:bg-blue-700 transition-colors"
        >
          <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
          </svg>
        </Link>
      </div>
    </div>
  );
};

export default ActivitySuggestions;