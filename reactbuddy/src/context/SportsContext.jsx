import React, { createContext, useContext, useReducer, useCallback } from 'react';
import { api } from '../services/ApiService';

// Define the initial state
const initialState = {
  sports: [], // Ensure this is always an array
  loading: false,
  error: null,
  selectedSport: null
};

// Create context
const SportsContext = createContext(initialState);

// Define reducer
const sportsReducer = (state, action) => {
  switch (action.type) {
    case 'FETCH_SPORTS_REQUEST':
      return { ...state, loading: true, error: null };
    case 'FETCH_SPORTS_SUCCESS':
      // Ensure we're setting an array to the sports property
      return { 
        ...state, 
        loading: false, 
        sports: Array.isArray(action.payload) ? action.payload : [] 
      };
    case 'FETCH_SPORTS_FAILURE':
      return { ...state, loading: false, error: action.payload, sports: [] }; // Reset to empty array on error
    case 'ADD_SPORT_SUCCESS':
      return { ...state, sports: [...state.sports, action.payload] };
    case 'UPDATE_SPORT_SUCCESS':
      return {
        ...state,
        sports: state.sports.map(sport =>
          sport.sportId === action.payload.sportId ? action.payload : sport
        )
      };
    case 'SELECT_SPORT':
      return { ...state, selectedSport: action.payload };
    default:
      return state;
  }
};

// Create provider component
export const SportsProvider = ({ children }) => {
  const [state, dispatch] = useReducer(sportsReducer, initialState);

  // Action creators - memoized with useCallback
  const fetchSports = useCallback(async () => {
    dispatch({ type: 'FETCH_SPORTS_REQUEST' });
    try {
      const response = await api.get('/api/Sports');
      // Handle different response structures
      let sportsData;
      if (response && response.data) {
        sportsData = response.data;
      } else if (Array.isArray(response)) {
        sportsData = response;
      } else {
        console.error('Unexpected response format:', response);
        sportsData = [];
      }
      
      dispatch({
        type: 'FETCH_SPORTS_SUCCESS',
        payload: sportsData
      });
    } catch (error) {
      console.error('Error fetching sports:', error);
      dispatch({
        type: 'FETCH_SPORTS_FAILURE',
        payload: error.message || 'Failed to fetch sports'
      });
    }
  }, []);

  const addSport = useCallback(async (sportData) => {
    try {
      const response = await api.post('/api/Sports', sportData);
      
      // Handle different response structures
      const newSport = response && response.data ? response.data : response;
      
      dispatch({
        type: 'ADD_SPORT_SUCCESS',
        payload: newSport
      });
      return newSport;
    } catch (error) {
      console.error('Error adding sport:', error);
      throw error;
    }
  }, []);

  const updateSport = useCallback(async (sportId, sportData) => {
    try {
      const response = await api.put(`/api/Sports/${sportId}`, sportData);
      
      // Handle different response structures
      const updatedSport = response && response.data ? response.data : response;
      
      dispatch({
        type: 'UPDATE_SPORT_SUCCESS',
        payload: updatedSport
      });
      return updatedSport;
    } catch (error) {
      console.error('Error updating sport:', error);
      throw error;
    }
  }, []);

  const selectSport = useCallback((sport) => {
    dispatch({ type: 'SELECT_SPORT', payload: sport });
  }, []);

  // Provide the state and actions
  const value = {
    ...state,
    fetchSports,
    addSport,
    updateSport,
    selectSport
  };

  return (
    <SportsContext.Provider value={value}>
      {children}
    </SportsContext.Provider>
  );
};

// Custom hook for consuming the context
export const useSports = () => {
  const context = useContext(SportsContext);
  if (context === undefined) {
    throw new Error('useSports must be used within a SportsProvider');
  }
  return context;
};

export default SportsContext;