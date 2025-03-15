import React, { createContext, useContext, useReducer, useEffect } from 'react';
import axios from 'axios';

// Define the initial state
const initialState = {
  sports: [],
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
      return { ...state, loading: false, sports: action.payload };
    case 'FETCH_SPORTS_FAILURE':
      return { ...state, loading: false, error: action.payload };
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
  const apiBaseUrl = import.meta.env.VITE_API_URL || 'https://api.buddyup.com';

  // Action creators
  const fetchSports = async () => {
    dispatch({ type: 'FETCH_SPORTS_REQUEST' });
    try {
      const response = await axios.get(`${apiBaseUrl}/api/Sports`);
      if (response.data.success) {
        dispatch({ 
          type: 'FETCH_SPORTS_SUCCESS', 
          payload: response.data.data 
        });
      } else {
        throw new Error(response.data.message || 'Failed to fetch sports');
      }
    } catch (error) {
      dispatch({ 
        type: 'FETCH_SPORTS_FAILURE', 
        payload: error.message 
      });
    }
  };

  const addSport = async (sportData) => {
    const response = await axios.post(`${apiBaseUrl}/api/Sports`, sportData, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });
    
    if (response.data.success) {
      dispatch({ 
        type: 'ADD_SPORT_SUCCESS', 
        payload: response.data.data 
      });
      return response.data.data;
    } else {
      throw new Error(response.data.message || 'Failed to add sport');
    }
  };

  const updateSport = async (sportId, sportData) => {
    const response = await axios.put(`${apiBaseUrl}/api/Sports/${sportId}`, sportData, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });
    
    if (response.data.success) {
      dispatch({ 
        type: 'UPDATE_SPORT_SUCCESS', 
        payload: response.data.data 
      });
      return response.data.data;
    } else {
      throw new Error(response.data.message || 'Failed to update sport');
    }
  };

  const selectSport = (sport) => {
    dispatch({ type: 'SELECT_SPORT', payload: sport });
  };

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