import React, { createContext, useContext, useState, useEffect, useCallback, useRef, useMemo } from 'react';
import { api } from '../services/ApiService';
import { useAuth } from './AuthContext';

// Create context
const AdminContext = createContext();

export const AdminProvider = ({ children }) => {
  const { isAuthenticated, token, isAdmin } = useAuth();
  
  const [users, setUsers] = useState({ items: [], totalItems: 0, totalPages: 0 });
  const [sports, setSports] = useState([]);
  const [locations, setLocations] = useState([]);
  const [dashboardStats, setDashboardStats] = useState({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  
  // Use ref to prevent duplicate API calls
  const pendingRequests = useRef({});
  
  // Skip duplicate state updates when the data hasn't changed
  const prevUsersRef = useRef(null);

  // Fetch dashboard stats on initial load
  useEffect(() => {
    if (isAuthenticated && isAdmin) {
      fetchDashboardStats();
    }
  }, [isAuthenticated, isAdmin]);

  // Fetch dashboard statistics
  const fetchDashboardStats = useCallback(async () => {
    if (!isAuthenticated || !isAdmin) return;
    
    const requestId = 'dashboard';
    if (pendingRequests.current[requestId]) return;
    
    pendingRequests.current[requestId] = true;
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.get('/api/admin/dashboard');
      
      if (response) {
        setDashboardStats(prev => ({...response}));
      }
    } catch (err) {
      console.error('Error fetching dashboard stats', err);
      setError('Failed to load dashboard statistics');
    } finally {
      setLoading(false);
      pendingRequests.current[requestId] = false;
    }
  }, [isAuthenticated, isAdmin]);

  // Fetch users with pagination and filters
  const fetchUsers = useCallback(async (page = 1, pageSize = 10, filters = {}) => {
    if (!isAdmin) return;
    
    // Create a unique request ID based on parameters
    const requestId = `users-${page}-${pageSize}-${JSON.stringify(filters)}`;
    
    // Prevent duplicate requests with same parameters
    if (pendingRequests.current[requestId]) {
      console.log('Skipping duplicate request:', requestId);
      return;
    }
    
    pendingRequests.current[requestId] = true;
    setLoading(true);
    setError(null);
    
    try {
      // Build URL with all required fields
      let baseUrl = `/api/admin/users?Page=${page}&PageSize=${pageSize}`;
      
      // IMPORTANT: Always include Search parameter (required by API validation)
      baseUrl += `&Search=${filters.search || '.'}`;
      
      // Add other filter parameters
      baseUrl += `&Sport=${filters.sport || 'all'}`;
      baseUrl += `&Status=${filters.status || ''}`;
      baseUrl += `&IsVerified=${filters.isVerified || ''}`;
      
      const response = await api.get(baseUrl);
      
      // Compare with previous state and only update if different
      if (!prevUsersRef.current || 
          JSON.stringify(prevUsersRef.current) !== JSON.stringify(response)) {
        
        // Create a completely new object to ensure React detects the change
        const newUsersState = response ? 
          {...response} : 
          { items: [], totalItems: 0, totalPages: 0 };
        
        prevUsersRef.current = newUsersState;
        setUsers(newUsersState);
      }
      return response;
    } catch (err) {
      console.error('Error fetching users:', err);
      setError('Failed to load users');
      
      // Initialize with empty dataset to avoid further errors
      setUsers({ items: [], totalItems: 0, totalPages: 0 });
      return { items: [], totalItems: 0, totalPages: 0 };
    } finally {
      setLoading(false);
      pendingRequests.current[requestId] = false;
    }
  }, [isAdmin]);

  // Update user active status
  const updateUserStatus = useCallback(async (userId, isActive) => {
    if (!isAuthenticated || !isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.put(`/api/admin/users/${userId}/status`, {
        IsActive: isActive
      });
      
      if (response) {
        // Create a completely new object and array to ensure React detects the change
        setUsers(prev => {
          const newUsers = {
            ...prev,
            items: prev.items.map(user => 
              user.userId === userId 
                ? { ...user, active: isActive } 
                : {...user}
            )
          };
          
          prevUsersRef.current = newUsers;
          return newUsers;
        });
      }
      
      return response;
    } catch (err) {
      console.error('Error updating user status', err);
      setError('Failed to update user status');
      throw err;
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, isAdmin]);

  // Fetch sports
  const fetchSports = useCallback(async () => {
    if (!isAuthenticated || !isAdmin) return;
    
    const requestId = 'sports';
    if (pendingRequests.current[requestId]) return;
    
    pendingRequests.current[requestId] = true;
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.get('/api/admin/sports');
      
      if (response) {
        setSports([...response]);
      }
    } catch (err) {
      console.error('Error fetching sports', err);
      setError('Failed to load sports');
    } finally {
      setLoading(false);
      pendingRequests.current[requestId] = false;
    }
  }, [isAuthenticated, isAdmin]);

  // Create a new sport
  const createSport = useCallback(async (sportData) => {
    if (!isAuthenticated || !isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      // Convert to pascal case for API
      const apiData = {
        Name: sportData.name,
        Description: sportData.description,
        IconUrl: sportData.iconUrl,
        Category: sportData.category,
        IsActive: sportData.isActive
      };
      
      const response = await api.post('/api/admin/sports', apiData);
      
      if (response) {
        setSports(prev => [...prev, response]);
      }
      
      return response;
    } catch (err) {
      console.error('Error creating sport', err);
      setError('Failed to create sport');
      throw err;
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, isAdmin]);

  // Update an existing sport
  const updateSport = useCallback(async (sportId, sportData) => {
    if (!isAuthenticated || !isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      // Convert to pascal case for API
      const apiData = {
        Name: sportData.name,
        Description: sportData.description,
        IconUrl: sportData.iconUrl,
        Category: sportData.category,
        IsActive: sportData.isActive
      };
      
      const response = await api.put(`/api/admin/sports/${sportId}`, apiData);
      
      if (response) {
        setSports(prev => 
          prev.map(sport => 
            sport.sportId === sportId ? {...response} : {...sport}
          )
        );
      }
      
      return response;
    } catch (err) {
      console.error('Error updating sport', err);
      setError('Failed to update sport');
      throw err;
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, isAdmin]);

  // Fetch locations
  const fetchLocations = useCallback(async () => {
    if (!isAuthenticated || !isAdmin) return;
    
    const requestId = 'locations';
    if (pendingRequests.current[requestId]) return;
    
    pendingRequests.current[requestId] = true;
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.get('/api/location');
      
      if (response) {
        setLocations([...response]);
      }
    } catch (err) {
      console.error('Error fetching locations', err);
      setError('Failed to load locations');
    } finally {
      setLoading(false);
      pendingRequests.current[requestId] = false;
    }
  }, [isAuthenticated, isAdmin]);

  // Create a new location
  const createLocation = useCallback(async (locationData) => {
    if (!isAuthenticated || !isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      // Convert to pascal case for API
      const apiData = {
        Name: locationData.name,
        Address: locationData.address,
        City: locationData.city,
        State: locationData.state,
        ZipCode: locationData.zipCode,
        Country: locationData.country,
        Latitude: locationData.latitude,
        Longitude: locationData.longitude,
        IsActive: locationData.isActive
      };
      
      const response = await api.post('/api/admin/locations', apiData);
      
      if (response) {
        setLocations(prev => [...prev, response]);
      }
      
      return response;
    } catch (err) {
      console.error('Error creating location', err);
      setError('Failed to create location');
      throw err;
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, isAdmin]);

  // Update an existing location
  const updateLocation = useCallback(async (locationId, locationData) => {
    if (!isAuthenticated || !isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      // Convert to pascal case for API
      const apiData = {
        Name: locationData.name,
        Address: locationData.address,
        City: locationData.city,
        State: locationData.state,
        ZipCode: locationData.zipCode,
        Country: locationData.country,
        Latitude: locationData.latitude,
        Longitude: locationData.longitude,
        IsActive: locationData.isActive
      };
      
      const response = await api.put(`/api/admin/locations/${locationId}`, apiData);
      
      if (response) {
        setLocations(prev => 
          prev.map(location => 
            location.locationId === locationId ? {...response} : {...location}
          )
        );
      }
      
      return response;
    } catch (err) {
      console.error('Error updating location', err);
      setError('Failed to update location');
      throw err;
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, isAdmin]);

  // Use useMemo to prevent unnecessary re-renders of consumers
  const adminValue = useMemo(() => ({
    users,
    sports,
    locations,
    dashboardStats,
    loading,
    error,
    fetchUsers,
    updateUserStatus,
    fetchSports,
    createSport,
    updateSport,
    fetchLocations,
    createLocation,
    updateLocation,
    fetchDashboardStats
  }), [
    users, 
    sports, 
    locations, 
    dashboardStats, 
    loading, 
    error, 
    fetchUsers,
    updateUserStatus,
    fetchSports,
    createSport,
    updateSport,
    fetchLocations,
    createLocation,
    updateLocation,
    fetchDashboardStats
  ]);

  return (
    <AdminContext.Provider value={adminValue}>
      {children}
    </AdminContext.Provider>
  );
};

// Custom hook for using Admin context
export const useAdmin = () => {
  const context = useContext(AdminContext);
  if (!context) {
    throw new Error('useAdmin must be used within an AdminProvider');
  }
  return context;
};

export default AdminContext;