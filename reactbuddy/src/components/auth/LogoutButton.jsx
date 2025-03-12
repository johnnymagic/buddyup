import React from 'react';
import { useAuth } from '../../hooks/useAuth';
import Button from '../common/Button';

/**
 * Logout button component that uses Auth0
 */
const LogoutButton = () => {
  const { logout } = useAuth();

  const handleLogout = () => {
    logout({
      returnTo: window.location.origin
    });
  };

  return (
    <Button 
      onClick={handleLogout}
      className="text-blue-100 hover:bg-blue-500"
    >
      Log Out
    </Button>
  );
};

export default LogoutButton;