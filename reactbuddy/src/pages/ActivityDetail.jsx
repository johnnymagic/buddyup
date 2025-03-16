import React, { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useActivity } from '../context/ActivityContext';
import { useAuth } from '../context/AuthContext';

const ActivityDetail = () => {
  const { activityId } = useParams();
  const navigate = useNavigate();
  const { getActivityById, updateActivity, deleteActivity, loading, error } = useActivity();
  const { userProfile } = useAuth();
  
  const [activity, setActivity] = useState(null);
  const [isOwner, setIsOwner] = useState(false);
  const [confirmDelete, setConfirmDelete] = useState(false);

  // Fetch activity on mount
  useEffect(() => {
    const fetchActivity = async () => {
      try {
        const activityData = await getActivityById(activityId);
        setActivity(activityData);
        
        // Check if current user is the owner
        setIsOwner(activityData.createdBy === userProfile?.Auth0UserId);
      } catch (err) {
        console.error('Failed to fetch activity', err);
      }
    };
    
    fetchActivity();
  }, [activityId, getActivityById, userProfile]);

  // Handle delete confirmation
  const handleDelete = async () => {
    if (!confirmDelete) {
      setConfirmDelete(true);
      return;
    }
    
    try {
      await deleteActivity(activityId);
      navigate('/activities');
    } catch (err) {
      console.error('Failed to delete activity', err);
    }
  };

  // Format date string
  const formatDate = (dateString) => {
    const options = { year: 'numeric', month: 'long', day: 'numeric' };
    return new Date(dateString).toLocaleDateString(undefined, options);
  };

  if (loading) {
    return (
      <div className="flex justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
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
        <Link to="/activities" className="text-blue-600 hover:text-blue-800">
          &larr; Back to Activities
        </Link>
      </div>
    );
  }

  if (!activity) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center py-12 bg-white rounded-lg shadow-md">
          <h3 className="text-lg font-medium text-gray-900">Activity not found</h3>
          <p className="mt-1 text-gray-500">
            The activity you're looking for doesn't exist or may have been removed.
          </p>
          <div className="mt-4">
            <Link
              to="/activities"
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              Browse Activities
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-6">
        <Link to="/activities" className="text-blue-600 hover:text-blue-800">
          &larr; Back to Activities
        </Link>
      </div>

      <div className="bg-white rounded-lg shadow-lg overflow-hidden mb-8">
        <div className="p-6">
          <div className="flex justify-between items-start mb-4">
            <h1 className="text-3xl font-bold text-gray-900">{activity.name}</h1>
            <span className={`px-3 py-1 text-sm rounded-full ${
              activity.difficultyLevel === 'beginner' ? 'bg-green-100 text-green-800' :
              activity.difficultyLevel === 'intermediate' ? 'bg-blue-100 text-blue-800' :
              activity.difficultyLevel === 'advanced' ? 'bg-orange-100 text-orange-800' :
              'bg-red-100 text-red-800'
            }`}>
              {activity.difficultyLevel}
            </span>
          </div>

          <div className="flex flex-wrap gap-4 mb-6">
            <div className="flex items-center text-gray-600">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              {activity.location?.name || 'Location not specified'}
            </div>
            
            <div className="flex items-center text-gray-600">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
              </svg>
              {activity.sport?.name}
            </div>
            
            {activity.distance && (
              <div className="flex items-center text-gray-600">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7" />
                </svg>
                {activity.distance.toFixed(1)} km away
              </div>
            )}
          </div>

          <div className="bg-gray-50 rounded-lg p-6 mb-6">
            <h2 className="text-xl font-semibold mb-3">Description</h2>
            <p className="text-gray-700">{activity.description}</p>
          </div>

          {activity.recurringSchedule && (
            <div className="mb-6">
              <h2 className="text-xl font-semibold mb-3">Schedule</h2>
              <div className="bg-blue-50 rounded-lg p-4">
                <div className="flex items-center">
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-2 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  <span className="text-blue-800">{activity.recurringSchedule}</span>
                </div>
              </div>
            </div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
            <div>
              <h2 className="text-xl font-semibold mb-3">Details</h2>
              <div className="bg-gray-50 rounded-lg p-4">
                <ul className="space-y-2">
                  {activity.maxParticipants && (
                    <li className="flex justify-between">
                      <span className="text-gray-600">Max Participants:</span>
                      <span className="font-medium">{activity.maxParticipants}</span>
                    </li>
                  )}
                  <li className="flex justify-between">
                    <span className="text-gray-600">Created:</span>
                    <span className="font-medium">{formatDate(activity.createdAt)}</span>
                  </li>
                  <li className="flex justify-between">
                    <span className="text-gray-600">Status:</span>
                    <span className={activity.isActive ? "text-green-600 font-medium" : "text-red-600 font-medium"}>
                      {activity.isActive ? "Active" : "Inactive"}
                    </span>
                  </li>
                </ul>
              </div>
            </div>

            <div>
              <h2 className="text-xl font-semibold mb-3">Contact</h2>
              <div className="bg-gray-50 rounded-lg p-4">
                <button
                  className="w-full py-2 bg-blue-600 text-white font-medium rounded hover:bg-blue-700 transition-colors"
                >
                  Contact Organizer
                </button>
              </div>
            </div>
          </div>

          {/* Participants section */}
          <div className="mb-6">
            <h2 className="text-xl font-semibold mb-3">Participants</h2>
            <div className="bg-gray-50 rounded-lg p-4">
              {activity.participants && activity.participants.length > 0 ? (
                <div className="space-y-2">
                  {activity.participants.map(participant => (
                    <div key={participant.userId} className="flex items-center justify-between p-2 hover:bg-gray-100 rounded">
                      <div className="flex items-center">
                        <div className="h-10 w-10 rounded-full bg-blue-500 flex items-center justify-center text-white font-medium">
                          {participant.displayName ? participant.displayName.charAt(0).toUpperCase() : '?'}
                        </div>
                        <div className="ml-3">
                          <p className="font-medium">{participant.displayName || 'Anonymous User'}</p>
                          <p className="text-sm text-gray-500">{participant.status}</p>
                        </div>
                      </div>
                      {isOwner && (
                        <button className="text-sm text-gray-600 hover:text-gray-900">
                          Message
                        </button>
                      )}
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-4">
                  <p className="text-gray-500">No participants yet</p>
                  {!isOwner && (
                    <button className="mt-2 px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded hover:bg-blue-700 transition-colors">
                      Join Activity
                    </button>
                  )}
                </div>
              )}
            </div>
          </div>

          {/* Owner actions */}
          {isOwner && (
            <div className="border-t border-gray-200 pt-6 mt-6">
              <h2 className="text-xl font-semibold mb-3">Activity Management</h2>
              <div className="flex flex-wrap gap-4">
                <Link
                  to={`/activities/edit/${activityId}`}
                  className="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded hover:bg-blue-700 transition-colors"
                >
                  Edit Activity
                </Link>
                
                {!confirmDelete ? (
                  <button
                    onClick={handleDelete}
                    className="px-4 py-2 bg-red-100 text-red-800 text-sm font-medium rounded hover:bg-red-200 transition-colors"
                  >
                    Delete Activity
                  </button>
                ) : (
                  <div className="flex items-center gap-2">
                    <button
                      onClick={handleDelete}
                      className="px-4 py-2 bg-red-600 text-white text-sm font-medium rounded hover:bg-red-700 transition-colors"
                    >
                      Confirm Delete
                    </button>
                    <button
                      onClick={() => setConfirmDelete(false)}
                      className="px-4 py-2 bg-gray-200 text-gray-800 text-sm font-medium rounded hover:bg-gray-300 transition-colors"
                    >
                      Cancel
                    </button>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ActivityDetail;