import { useContext } from 'react';
import MatchingContext from '../context/MatchingContext';

/**
 * Custom hook to access the matching context
 * 
 * @returns {Object} Matching context values and functions
 */
export const useMatching = () => {
  const context = useContext(MatchingContext);
  
  if (!context) {
    throw new Error('useMatching must be used within a MatchingProvider');
  }
  
  return context;
};

export default useMatching;