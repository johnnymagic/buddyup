import React, { createContext, useContext, useState, useEffect } from 'react';
import { api } from '../services/ApiService';
import { useAuth } from './AuthContext';
import { useProfile } from './ProfileContext';

// Create context
const ActivityContext = createContext();

export const ActivityProvider = ({ children }) => {
  const { isAuthenticated, token } = useAuth();
  const { profileData } = useProfile();
  
  const [activities, setActivities] = useState([]);
  const [userActivities, setUserActivities] = useState([]);
  const [recommendedActivities, setRecommendedActivities] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  // Initialize state with default filters
  const [filter, setFilter] = useState({
    sportId: '',
    latitude: null,
    longitude: null,
    distance: 25, // km
    difficulty: 'all', // Initialize with 'all' instead of empty string
    page: 1,
    pageSize: 10
  });

  // Update location from profile when available
  useEffect(() => {
    if (profileData?.latitude && profileData?.longitude) {
      setFilter(prev => ({
        ...prev,
        latitude: profileData.latitude,
        longitude: profileData.longitude
      }));
    }
  }, [profileData]);

  // Fetch activities when filter changes or on initial load
  useEffect(() => {
    if (isAuthenticated && token && filter.latitude && filter.longitude) {
      fetchActivities();
    }
  }, [isAuthenticated, token, filter]);

  // Fetch activities based on current filters
  const fetchActivities = async () => {
    if (!isAuthenticated || !token) return;
    
    setLoading(true);
    setError(null);
    
    try {
      // Create a query string manually to ensure correct casing
      let queryString = '?';
      
      // Add parameters with correct casing
      if (filter.sportId) queryString += `SportId=${filter.sportId}&`;
      if (filter.latitude) queryString += `Latitude=${filter.latitude}&`;
      if (filter.longitude) queryString += `Longitude=${filter.longitude}&`;
      if (filter.distance) queryString += `Distance=${filter.distance}&`;
      
      // Only add specific difficulty if not 'all'
      if (filter.difficulty && filter.difficulty !== 'all') {
        queryString += `Difficulty=${filter.difficulty}&`;
      } else {
        // Add empty Difficulty parameter as required by API
        queryString += `Difficulty=all&`;
      }
      
      queryString += `Page=${filter.page}&PageSize=${filter.pageSize}`;
      
      const response = await api.get(`/api/activity${queryString}`);
      
      if (response && response.data) {
        setActivities(response.data);
      } else {
        setActivities([]);
      }
    } catch (err) {
      console.error('Error fetching activities', err);
      setError('Failed to load activities');
    } finally {
      setLoading(false);
    }
  };

  // Fetch user's own activities
  const fetchUserActivities = async () => {
    if (!isAuthenticated || !token) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.get('/api/activity/my');
      
      if (response && response.data) {
        setUserActivities(response.data);
      } else {
        setUserActivities([]);
      }
    } catch (err) {
      console.error('Error fetching user activities', err);
      setError('Failed to load your activities');
    } finally {
      setLoading(false);
    }
  };

  // Fetch recommended activities based on user profile and preferences
  const fetchRecommendedActivities = async () => {
    if (!isAuthenticated || !token) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.get('/api/activity/recommended');
      
      if (response && response.data) {
        setRecommendedActivities(response.data);
      } else {
        setRecommendedActivities([]);
      }
    } catch (err) {
      console.error('Error fetching recommended activities', err);
      setError('Failed to load recommendations');
    } finally {
      setLoading(false);
    }
  };

  // Create a new activity
  const createActivity = async (activityData) => {
    if (!isAuthenticated || !token) return;
    
    setLoading(true);
    setError(null);
    
    try {
      // Convert to pascal case for API
      const apiData = {
        SportId: activityData.sportId,
        LocationId: activityData.locationId,
        Name: activityData.name,
        Description: activityData.description,
        RecurringSchedule: activityData.recurringSchedule,
        DifficultyLevel: activityData.difficultyLevel,
        MaxParticipants: activityData.maxParticipants,
        IsActive: activityData.isActive
      };
      
      const response = await api.post('/api/activity', apiData);
      
      // Add new activity to user activities list
      if (response) {
        setUserActivities(prev => [response, ...prev]);
      }
      
      return response;
    } catch (err) {
      console.error('Error creating activity', err);
      setError('Failed to create activity');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Update an existing activity
  const updateActivity = async (activityId, activityData) => {
    if (!isAuthenticated || !token) return;
    
    setLoading(true);
    setError(null);
    
    try {
      // Convert to pascal case for API
      const apiData = {
        SportId: activityData.sportId,
        LocationId: activityData.locationId,
        Name: activityData.name,
        Description: activityData.description,
        RecurringSchedule: activityData.recurringSchedule,
        DifficultyLevel: activityData.difficultyLevel,
        MaxParticipants: activityData.maxParticipants,
        IsActive: activityData.isActive
      };
      
      const response = await api.put(`/api/activity/${activityId}`, apiData);
      
      // Update activity in lists if it exists
      if (response) {
        setUserActivities(prev => 
          prev.map(activity => 
            activity.activityId === activityId ? response : activity
          )
        );
        
        setActivities(prev => 
          prev.map(activity => 
            activity.activityId === activityId ? response : activity
          )
        );
        
        setRecommendedActivities(prev => 
          prev.map(activity => 
            activity.activityId === activityId ? response : activity
          )
        );
      }
      
      return response;
    } catch (err) {
      console.error('Error updating activity', err);
      setError('Failed to update activity');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Delete an activity
  const deleteActivity = async (activityId) => {
    if (!isAuthenticated || !token) return;
    
    setLoading(true);
    setError(null);
    
    try {
      await api.delete(`/api/activity/${activityId}`);
      
      // Remove activity from all lists
      setUserActivities(prev => 
        prev.filter(activity => activity.activityId !== activityId)
      );
      
      setActivities(prev => 
        prev.filter(activity => activity.activityId !== activityId)
      );
      
      setRecommendedActivities(prev => 
        prev.filter(activity => activity.activityId !== activityId)
      );
      
      return true;
    } catch (err) {
      console.error('Error deleting activity', err);
      setError('Failed to delete activity');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Get a single activity by ID
  const getActivityById = async (activityId) => {
    if (!isAuthenticated || !token) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.get(`/api/activity/${activityId}`);
      return response && response.data ? response.data : null;
    } catch (err) {
      console.error('Error fetching activity details', err);
      setError('Failed to load activity details');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Update filter values
  const updateFilter = (newFilterValues) => {
    setFilter(prev => ({
      ...prev,
      ...newFilterValues
    }));
  };

  // Reset filters to default values
  const resetFilters = () => {
    setFilter({
      sportId: '',
      latitude: profileData?.latitude || null,
      longitude: profileData?.longitude || null,
      distance: 25,
      difficulty: '',
      page: 1,
      pageSize: 10
    });
  };

  // Value object to be provided to consumers
  const activityValue = {
    activities,
    userActivities,
    recommendedActivities,
    loading,
    error,
    filter,
    fetchActivities,
    fetchUserActivities,
    fetchRecommendedActivities,
    createActivity,
    updateActivity,
    deleteActivity,
    getActivityById,
    updateFilter,
    resetFilters
  };

  return (
    <ActivityContext.Provider value={activityValue}>
      {children}
    </ActivityContext.Provider>
  );
};

// Custom hook for using Activity context
export const useActivity = () => {
  const context = useContext(ActivityContext);
  if (!context) {
    throw new Error('useActivity must be used within an ActivityProvider');
  }
  return context;
};

export default ActivityContext;