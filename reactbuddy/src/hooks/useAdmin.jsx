import { useContext } from 'react';
import AdminContext from '../context/AdminContext';

/**
 * Custom hook to access the admin context
 * 
 * @returns {Object} Admin context values and functions
 */
export const useAdmin = () => {
  const context = useContext(AdminContext);
  
  if (!context) {
    throw new Error('useAdmin must be used within an AdminProvider');
  }
  
  return context;
};

export default useAdmin;