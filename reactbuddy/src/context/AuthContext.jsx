import React, { createContext, useContext, useState, useEffect } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import { api } from '../services/ApiService';

// Create context
const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const {
    isLoading: auth0Loading,
    isAuthenticated,
    user,
    getAccessTokenSilently,
    loginWithRedirect,
    logout,
    getIdTokenClaims
  } = useAuth0();

  const [token, setToken] = useState(null);
  const [userProfile, setUserProfile] = useState(null);
  const [isAdmin, setIsAdmin] = useState(false);
  const [isVerified, setIsVerified] = useState(false);
  const [authLoading, setAuthLoading] = useState(true);
  const [tokenError, setTokenError] = useState(null);
  const [isProfileLoaded, setIsProfileLoaded] = useState(false);
  const [profileError, setProfileError] = useState(null);
  const [sessionChecked, setSessionChecked] = useState(false);

  // Check for existing session on initial load
  useEffect(() => {
    // This will force Auth0 to check for an existing session
    // and automatically log the user in if it exists
    const checkSession = async () => {
      try {
        await getIdTokenClaims();
      } catch (error) {
        console.log('No existing session found or error checking session');
      } finally {
        setSessionChecked(true);
      }
    };

    checkSession();
  }, [getIdTokenClaims]);

  // Get token when user authenticates
  useEffect(() => {
    const getToken = async () => {
      if (!isAuthenticated || !user) {
        setAuthLoading(false);
        return;
      }
      
      try {
        // Get Auth0 access token with the correct audience
        const accessToken = await getAccessTokenSilently({
          authorizationParams: {
            audience: import.meta.env.VITE_AUTH0_AUDIENCE,
            scope: 'openid profile email'
          },
          cacheMode: 'cache-first', // Prioritize using cached token
        });

        setToken(accessToken);
        // Set token in API service
        api.setAuthToken(accessToken);
        
        // Attempt to fetch user profile
        try {
          const profile = await api.get('/api/profile');
          console.log('Profile fetched:', profile);

          if (profile) {
            setUserProfile(profile);
            setIsAdmin(profile.isAdmin || false);
            setIsVerified(profile.verificationStatus === 'Verified' || false);
          }
          
          setIsProfileLoaded(true);
        } catch (error) {
          console.error('Error fetching profile:', error);
          setProfileError(error);
          
          // We consider the profile "loaded" even if it doesn't exist yet
          // This allows our UI to properly show the profile creation form
          if (error.status === 404) {
            console.log('Profile not found, but that\'s ok for new users');
            setIsProfileLoaded(true);
          }
        }
      } catch (error) {
        console.error('Error getting authentication token:', error);
        setTokenError(error.message);
      } finally {
        setAuthLoading(false);
      }
    };

    if (sessionChecked) {
      getToken();
    }
  }, [isAuthenticated, user, getAccessTokenSilently, sessionChecked]);

  // Clear state on logout
  const handleLogout = () => {
    console.log('Logging out user');
    setToken(null);
    setUserProfile(null);
    setIsAdmin(false);
    setIsVerified(false);
    setIsProfileLoaded(false);
    api.clearAuthToken();
    logout({
      logoutParams: {
        returnTo: window.location.origin
      }
    });
  };

  // Update profile with intelligent create/update logic
  const updateProfile = async (profileData) => {
    try {
      console.log('Updating profile with data:', profileData);

      // Ensure we have a token
      if (!token) {
        console.log('No token available, refreshing token...');
        const newToken = await getAccessTokenSilently({
          authorizationParams: {
            audience: import.meta.env.VITE_AUTH0_AUDIENCE
          }
        });
        setToken(newToken);
        api.setAuthToken(newToken);
      }

      // Ensure required fields are present
      const updatedProfileData = {
        ...profileData,
        // Add Auth0 user ID if not present
        Auth0UserId: profileData.Auth0UserId || user?.sub || '',
        // Add profileId if we have one and it's not in the data
        ProfileId: userProfile?.profileId || profileData.ProfileId || null,
        // Ensure verification status is set
        VerificationStatus: profileData.VerificationStatus || userProfile?.verificationStatus || 'Unverified',
        // Ensure User ID is set (internal database ID)
        UserId: userProfile?.userId || profileData.UserId || '',
        // Ensure we have default preferences if not provided
        PreferredDays: profileData.PreferredDays || userProfile?.preferredDays || [],
        PreferredTimes: profileData.PreferredTimes || userProfile?.preferredTimes || [],
        // Ensure we have location data
        Latitude: profileData.Latitude || 0,
        Longitude: profileData.Longitude || 0,
        PublicProfile: profileData.PublicProfile !== undefined ? profileData.PublicProfile : true
      };

      let result;
      
      // Choose whether to create or update based on profile existence
      if (userProfile) {
        console.log('Updating existing profile');
        result = await api.put('/api/profile', updatedProfileData);
      } else {
        console.log('Creating new profile');
        result = await api.post('/api/profile', updatedProfileData);
      }
      
      console.log('Profile saved successfully:', result);
      
      // Update local state with the new profile data
      setUserProfile(result);
      setIsVerified(result.verificationStatus === 'Verified');
      setIsProfileLoaded(true);
      
      return result;
    } catch (error) {
      console.error('Error updating profile:', error);

      // If the error is due to token expiration, try to refresh the token and retry
      if (error.status === 401) {
        try {
          console.log('Token expired, refreshing...');
          const newToken = await getAccessTokenSilently({
            authorizationParams: {
              audience: import.meta.env.VITE_AUTH0_AUDIENCE
            }
          });
          setToken(newToken);
          api.setAuthToken(newToken);

          // Retry the update with new token
          if (userProfile) {
            const updatedProfile = await api.put('/api/profile', profileData);
            setUserProfile(updatedProfile);
            return updatedProfile;
          } else {
            const newProfile = await api.post('/api/profile', profileData);
            setUserProfile(newProfile);
            return newProfile;
          }
        } catch (refreshError) {
          console.error('Failed to refresh token:', refreshError);
          throw refreshError;
        }
      }

      // If we get a 404 on PUT, try POST as a fallback (this shouldn't happen with our new logic, but just in case)
      if (error.status === 404 && !userProfile) {
        try {
          console.log('Profile not found, creating new profile...');
          const newProfile = await api.post('/api/profile', profileData);
          setUserProfile(newProfile);
          setIsProfileLoaded(true);
          return newProfile;
        } catch (createError) {
          console.error('Error creating profile:', createError);
          throw createError;
        }
      }

      throw error;
    }
  };

  // Value object to be provided to consumers
  const authValue = {
    isLoading: auth0Loading || authLoading || !sessionChecked,
    isAuthenticated,
    user,
    token,
    userProfile,
    isAdmin,
    isVerified,
    tokenError,
    isProfileLoaded,
    profileError,
    hasProfile: isProfileLoaded && userProfile !== null,
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