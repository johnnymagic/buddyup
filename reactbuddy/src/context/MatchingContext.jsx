import React, { createContext, useContext, useState, useEffect } from 'react';
import { api } from '../services/ApiService';
import { useAuth } from './AuthContext';

// Create context
const MatchingContext = createContext();

export const MatchingProvider = ({ children }) => {
  const { isAuthenticated, userProfile } = useAuth();
  
  const [potentialMatches, setPotentialMatches] = useState([]);
  const [currentMatches, setCurrentMatches] = useState([]);
  const [pendingRequests, setPendingRequests] = useState([]);
  const [receivedRequests, setReceivedRequests] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    sport: '',
    skillLevel: '',
    distance: 50, // default to 50km
    days: [],
    times: []
  });

  // Fetch potential matches when user is authenticated and filters change
  useEffect(() => {
    const fetchPotentialMatches = async () => {
      if (!isAuthenticated || !userProfile) return;
      
      setLoading(true);
      setError(null);
      
      try {
        const queryParams = new URLSearchParams();
        if (filters.sport) queryParams.append('sportId', filters.sport);
        if (filters.skillLevel) queryParams.append('skillLevel', filters.skillLevel);
        if (filters.distance) queryParams.append('distance', filters.distance);
        if (filters.days.length) queryParams.append('days', filters.days.join(','));
        if (filters.times.length) queryParams.append('times', filters.times.join(','));
        
        const response = await api.get(`/api/matching/potential?${queryParams.toString()}`);
        setPotentialMatches(response);
      } catch (err) {
        console.error('Error fetching potential matches', err);
        setError('Failed to load potential matches');
      } finally {
        setLoading(false);
      }
    };
    
    if (isAuthenticated && userProfile) {
      fetchPotentialMatches();
    }
  }, [isAuthenticated, userProfile, filters]);

  // Fetch current matches and requests
  useEffect(() => {
    const fetchMatchData = async () => {
      if (!isAuthenticated || !userProfile) return;
      
      setLoading(true);
      
      try {
        // Get current (accepted) matches
        const matchesResponse = await api.get('/api/matching/current');
        setCurrentMatches(matchesResponse);
        
        // Get sent match requests
        const pendingResponse = await api.get('/api/matching/sent');
        setPendingRequests(pendingResponse);
        
        // Get received match requests
        const receivedResponse = await api.get('/api/matching/received');
        setReceivedRequests(receivedResponse);
      } catch (err) {
        console.error('Error fetching match data', err);
        setError('Failed to load match data');
      } finally {
        setLoading(false);
      }
    };
    
    if (isAuthenticated && userProfile) {
      fetchMatchData();
    }
  }, [isAuthenticated, userProfile]);

  // Send a match request
  const sendMatchRequest = async (recipientId, sportId) => {
    setLoading(true);
    try {
      const response = await api.post('/api/matching/request', {
        recipientId,
        sportId
      });
      
      // Add to pending requests
      setPendingRequests(prev => [...prev, response]);
      
      // Remove from potential matches
      setPotentialMatches(prev => 
        prev.filter(match => match.userId !== recipientId || match.sportId !== sportId)
      );
      
      return response;
    } catch (err) {
      console.error('Error sending match request', err);
      setError('Failed to send match request');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Respond to a match request
  const respondToRequest = async (matchId, accept) => {
    setLoading(true);
    try {
      const response = await api.put(`/api/matching/${matchId}/respond`, {
        accept
      });
      
      // Remove from received requests
      setReceivedRequests(prev => 
        prev.filter(request => request.matchId !== matchId)
      );
      
      // If accepted, add to current matches
      if (accept) {
        setCurrentMatches(prev => [...prev, response]);
      }
      
      return response;
    } catch (err) {
      console.error('Error responding to match request', err);
      setError('Failed to respond to match request');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Cancel a sent request
  const cancelRequest = async (matchId) => {
    setLoading(true);
    try {
      await api.delete(`/api/matching/${matchId}`);
      
      // Remove from pending requests
      setPendingRequests(prev => 
        prev.filter(request => request.matchId !== matchId)
      );
      
      // Get potential match details to add back to potentials
      const match = pendingRequests.find(r => r.matchId === matchId);
      if (match) {
        // Refresh potential matches
        const queryParams = new URLSearchParams();
        if (filters.sport) queryParams.append('sportId', filters.sport);
        if (filters.skillLevel) queryParams.append('skillLevel', filters.skillLevel);
        if (filters.distance) queryParams.append('distance', filters.distance);
        
        const potentials = await api.get(`/api/matching/potential?${queryParams.toString()}`);
        setPotentialMatches(potentials);
      }
    } catch (err) {
      console.error('Error canceling match request', err);
      setError('Failed to cancel match request');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Unmatch from a current match
  const unmatch = async (matchId) => {
    setLoading(true);
    try {
      await api.delete(`/api/matching/${matchId}`);
      
      // Remove from current matches
      setCurrentMatches(prev => 
        prev.filter(match => match.matchId !== matchId)
      );
    } catch (err) {
      console.error('Error unmatching', err);
      setError('Failed to unmatch');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Update filters
  const updateFilters = (newFilters) => {
    setFilters(prev => ({
      ...prev,
      ...newFilters
    }));
  };

  // Value object to be provided to consumers
  const matchingValue = {
    potentialMatches,
    currentMatches,
    pendingRequests,
    receivedRequests,
    loading,
    error,
    filters,
    updateFilters,
    sendMatchRequest,
    respondToRequest,
    cancelRequest,
    unmatch
  };

  return (
    <MatchingContext.Provider value={matchingValue}>
      {children}
    </MatchingContext.Provider>
  );
};

// Custom hook for using Matching context
export const useMatching = () => {
  const context = useContext(MatchingContext);
  if (!context) {
    throw new Error('useMatching must be used within a MatchingProvider');
  }
  return context;
};

export default MatchingContext;