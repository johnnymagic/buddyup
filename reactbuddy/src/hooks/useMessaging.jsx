import { useContext } from 'react';
import MessagingContext from '../context/MessagingContext';

/**
 * Custom hook to access the messaging context
 * 
 * @returns {Object} Messaging context values and functions
 */
export const useMessaging = () => {
  const context = useContext(MessagingContext);
  
  if (!context) {
    throw new Error('useMessaging must be used within a MessagingProvider');
  }
  
  return context;
};

export default useMessaging;