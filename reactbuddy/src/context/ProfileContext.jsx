import React, { createContext, useContext, useState, useEffect } from 'react';
import { api } from '../services/ApiService';
import { useAuth } from './AuthContext';

// Create context
const ProfileContext = createContext();

export const ProfileProvider = ({ children }) => {
  const { isAuthenticated, userProfile: authUserProfile } = useAuth();
  
  const [profileData, setProfileData] = useState(null);
  const [userSports, setUserSports] = useState([]);
  const [availableSports, setAvailableSports] = useState([]);
  const [verificationStatus, setVerificationStatus] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // Fetch profile data when authenticated
  useEffect(() => {
    const fetchProfileData = async () => {
      if (!isAuthenticated || !authUserProfile) return;

      setLoading(true);
      setError(null);
      
      try {
        // Fetch detailed profile data
        const profile = await api.get('/api/profile');
        setProfileData(profile);
        
        // Fetch user sports
        const sports = await api.get('/api/profile/sports');
        setUserSports(sports);
        
        // Fetch verification status
        const verification = await api.get('/api/profile/verification');
        setVerificationStatus(verification);
      } catch (err) {
        console.error('Error fetching profile data', err);
        setError('Failed to load profile data');
      } finally {
        setLoading(false);
      }
    };
    
    fetchProfileData();
  }, [isAuthenticated, authUserProfile]);

  // Fetch available sports
  useEffect(() => {
    const fetchSports = async () => {
      try {
        const sports = await api.get('/api/sports');
        setAvailableSports(sports);
      } catch (err) {
        console.error('Error fetching available sports', err);
      }
    };
    
    fetchSports();
  }, []);

  // Update profile
  const updateProfile = async (profileUpdateData) => {
    setLoading(true);
    setError(null);
    
    try {
      const updatedProfile = await api.put('/api/profile', profileUpdateData);
      setProfileData(updatedProfile);
      return updatedProfile;
    } catch (err) {
      console.error('Error updating profile', err);
      setError('Failed to update profile');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Add a sport to user profile
  const addSport = async (sportData) => {
    setLoading(true);
    setError(null);
    
    try {
      const newUserSport = await api.post('/api/profile/sports', sportData);
      setUserSports(prev => [...prev, newUserSport]);
      return newUserSport;
    } catch (err) {
      console.error('Error adding sport', err);
      setError('Failed to add sport');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Update a user sport
  const updateSport = async (userSportId, sportUpdateData) => {
    setLoading(true);
    setError(null);
    
    try {
      const updatedUserSport = await api.put(`/api/profile/sports/${userSportId}`, sportUpdateData);
      setUserSports(prev => 
        prev.map(sport => 
          sport.userSportId === userSportId ? updatedUserSport : sport
        )
      );
      return updatedUserSport;
    } catch (err) {
      console.error('Error updating sport', err);
      setError('Failed to update sport');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Remove a sport from user profile
  const removeSport = async (userSportId) => {
    setLoading(true);
    setError(null);
    
    try {
      await api.delete(`/api/profile/sports/${userSportId}`);
      setUserSports(prev => 
        prev.filter(sport => sport.userSportId !== userSportId)
      );
    } catch (err) {
      console.error('Error removing sport', err);
      setError('Failed to remove sport');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Initiate verification
  const initiateVerification = async (verificationType, provider) => {
    setLoading(true);
    setError(null);
    
    try {
      const verification = await api.post('/api/profile/verification/initiate', {
        verificationType,
        verificationProvider: provider
      });
      setVerificationStatus(verification);
      return verification;
    } catch (err) {
      console.error('Error initiating verification', err);
      setError('Failed to initiate verification');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Value object to be provided to consumers
  const profileValue = {
    profileData,
    userSports,
    availableSports,
    verificationStatus,
    loading,
    error,
    updateProfile,
    addSport,
    updateSport,
    removeSport,
    initiateVerification
  };

  return (
    <ProfileContext.Provider value={profileValue}>
      {children}
    </ProfileContext.Provider>
  );
};

// Custom hook for using Profile context
export const useProfile = () => {
  const context = useContext(ProfileContext);
  if (!context) {
    throw new Error('useProfile must be used within a ProfileProvider');
  }
  return context;
};

export default ProfileContext;