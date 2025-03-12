import React, { createContext, useContext, useState, useEffect } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import { api } from '../services/ApiService';

// Create context
const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const { 
    isLoading, 
    isAuthenticated, 
    user, 
    getAccessTokenSilently, 
    loginWithRedirect, 
    logout 
  } = useAuth0();
  
  const [token, setToken] = useState(null);
  const [userProfile, setUserProfile] = useState(null);
  const [isAdmin, setIsAdmin] = useState(false);
  const [isVerified, setIsVerified] = useState(false);
  const [authLoading, setAuthLoading] = useState(true);

  // Get token when user authenticates
  useEffect(() => {
    const getToken = async () => {
      if (isAuthenticated && user) {
        try {
          const accessToken = await getAccessTokenSilently();
          setToken(accessToken);
          
          // Set token in API service
          api.setAuthToken(accessToken);
          
          // Fetch user profile from our backend
          const profile = await api.get('/api/profile');
          if (profile) {
            setUserProfile(profile);
            setIsAdmin(profile.isAdmin || false);
            setIsVerified(profile.isVerified || false);
          } else {
            // If no profile exists, may need to create one
            await api.post('/api/profile', {
              auth0Id: user.sub,
              email: user.email,
              firstName: user.given_name || user.name.split(' ')[0],
              lastName: user.family_name || user.name.split(' ').slice(1).join(' ')
            });
            
            // Fetch the newly created profile
            const newProfile = await api.get('/api/profile');
            setUserProfile(newProfile);
          }
        } catch (error) {
          console.error('Error setting up authentication', error);
        } finally {
          setAuthLoading(false);
        }
      } else {
        setAuthLoading(false);
      }
    };

    getToken();
  }, [isAuthenticated, user, getAccessTokenSilently]);

  // Clear state on logout
  const handleLogout = () => {
    setToken(null);
    setUserProfile(null);
    setIsAdmin(false);
    setIsVerified(false);
    api.clearAuthToken();
    logout({ returnTo: window.location.origin });
  };

  // Update profile
  const updateProfile = async (profileData) => {
    try {
      const updatedProfile = await api.put('/api/profile', profileData);
      setUserProfile(updatedProfile);
      return updatedProfile;
    } catch (error) {
      console.error('Error updating profile', error);
      throw error;
    }
  };

  // Value object to be provided to consumers
  const authValue = {
    isLoading: isLoading || authLoading,
    isAuthenticated,
    user,
    token,
    userProfile,
    isAdmin,
    isVerified,
    login: loginWithRedirect,
    logout: handleLogout,
    updateProfile
  };

  return (
    <AuthContext.Provider value={authValue}>
      {children}
    </AuthContext.Provider>
  );
};

// Custom hook for using Auth context
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export default AuthContext;