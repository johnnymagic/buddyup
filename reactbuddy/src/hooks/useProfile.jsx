import { useContext } from 'react';
import ProfileContext from '../context/ProfileContext';

/**
 * Custom hook to access the profile context
 * 
 * @returns {Object} Profile context values and functions
 */
export const useProfile = () => {
  const context = useContext(ProfileContext);
  
  if (!context) {
    throw new Error('useProfile must be used within a ProfileProvider');
  }
  
  return context;
};

export default useProfile;