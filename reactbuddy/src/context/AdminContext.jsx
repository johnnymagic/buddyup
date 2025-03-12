import React, { createContext, useContext, useState, useEffect } from 'react';
import { api } from '../services/ApiService';
import { useAuth } from './AuthContext';

// Create context
const AdminContext = createContext();

export const AdminProvider = ({ children }) => {
  const { isAuthenticated, isAdmin } = useAuth();
  
  const [users, setUsers] = useState([]);
  const [sports, setSports] = useState([]);
  const [locations, setLocations] = useState([]);
  const [userReports, setUserReports] = useState([]);
  const [verifications, setVerifications] = useState([]);
  const [dashboardStats, setDashboardStats] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  
  // Load admin data when authenticated and has admin role
  useEffect(() => {
    const fetchAdminData = async () => {
      if (!isAuthenticated || !isAdmin) return;
      
      setLoading(true);
      setError(null);
      
      try {
        const stats = await api.get('/api/admin/stats');
        setDashboardStats(stats);
      } catch (err) {
        console.error('Error fetching admin dashboard stats', err);
        setError('Failed to load admin dashboard stats');
      } finally {
        setLoading(false);
      }
    };
    
    fetchAdminData();
  }, [isAuthenticated, isAdmin]);

  // User management functions
  const fetchUsers = async (page = 1, pageSize = 10, filters = {}) => {
    if (!isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const queryParams = new URLSearchParams();
      queryParams.append('page', page);
      queryParams.append('pageSize', pageSize);
      
      // Add filters to query
      Object.entries(filters).forEach(([key, value]) => {
        if (value) queryParams.append(key, value);
      });
      
      const response = await api.get(`/api/admin/users?${queryParams.toString()}`);
      setUsers(response);
      return response;
    } catch (err) {
      console.error('Error fetching users', err);
      setError('Failed to load users');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const updateUserStatus = async (userId, active) => {
    if (!isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.put(`/api/admin/users/${userId}/status`, { active });
      
      // Update user in list
      setUsers(prev => 
        prev.map(user => 
          user.userId === userId ? { ...user, active } : user
        )
      );
      
      return response;
    } catch (err) {
      console.error('Error updating user status', err);
      setError('Failed to update user status');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Sport management functions
  const fetchSports = async () => {
    if (!isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.get('/api/admin/sports');
      setSports(response);
      return response;
    } catch (err) {
      console.error('Error fetching sports', err);
      setError('Failed to load sports');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const addSport = async (sportData) => {
    if (!isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.post('/api/admin/sports', sportData);
      setSports(prev => [...prev, response]);
      return response;
    } catch (err) {
      console.error('Error adding sport', err);
      setError('Failed to add sport');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const updateSport = async (sportId, sportData) => {
    if (!isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.put(`/api/admin/sports/${sportId}`, sportData);
      
      // Update sport in list
      setSports(prev => 
        prev.map(sport => 
          sport.sportId === sportId ? response : sport
        )
      );
      
      return response;
    } catch (err) {
      console.error('Error updating sport', err);
      setError('Failed to update sport');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Location management functions
  const fetchLocations = async (page = 1, pageSize = 10) => {
    if (!isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.get(`/api/admin/locations?page=${page}&pageSize=${pageSize}`);
      setLocations(response);
      return response;
    } catch (err) {
      console.error('Error fetching locations', err);
      setError('Failed to load locations');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const addLocation = async (locationData) => {
    if (!isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.post('/api/admin/locations', locationData);
      setLocations(prev => [...prev, response]);
      return response;
    } catch (err) {
      console.error('Error adding location', err);
      setError('Failed to add location');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // User reports management
  const fetchUserReports = async (status = 'Pending') => {
    if (!isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.get(`/api/admin/reports?status=${status}`);
      setUserReports(response);
      return response;
    } catch (err) {
      console.error('Error fetching user reports', err);
      setError('Failed to load user reports');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const handleUserReport = async (reportId, action, notes) => {
    if (!isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.put(`/api/admin/reports/${reportId}`, {
        action,
        notes
      });
      
      // Update report in list
      setUserReports(prev => 
        prev.map(report => 
          report.reportId === reportId ? response : report
        )
      );
      
      return response;
    } catch (err) {
      console.error('Error handling user report', err);
      setError('Failed to handle user report');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Verification management
  const fetchVerifications = async (status = 'Pending') => {
    if (!isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.get(`/api/admin/verifications?status=${status}`);
      setVerifications(response);
      return response;
    } catch (err) {
      console.error('Error fetching verifications', err);
      setError('Failed to load verifications');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const handleVerification = async (verificationId, approved, notes) => {
    if (!isAdmin) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await api.put(`/api/admin/verifications/${verificationId}`, {
        approved,
        notes
      });
      
      // Update verification in list
      setVerifications(prev => 
        prev.map(verification => 
          verification.verificationId === verificationId ? response : verification
        )
      );
      
      return response;
    } catch (err) {
      console.error('Error handling verification', err);
      setError('Failed to handle verification');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Value object to be provided to consumers
  const adminValue = {
    users,
    sports,
    locations,
    userReports,
    verifications,
    dashboardStats,
    loading,
    error,
    fetchUsers,
    updateUserStatus,
    fetchSports,
    addSport,
    updateSport,
    fetchLocations,
    addLocation,
    fetchUserReports,
    handleUserReport,
    fetchVerifications,
    handleVerification
  };

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