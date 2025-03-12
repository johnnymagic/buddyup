import React from 'react';
import { useAuth } from '../../hooks/useAuth';
import Button from '../common/Button';

/**
 * Login button component that uses Auth0
 */
const LoginButton = () => {
  const { login } = useAuth();

  const handleLogin = () => {
    login({
      redirectUri: window.location.origin
    });
  };

  return (
    <Button 
      onClick={handleLogin}
      className="bg-white text-blue-600 hover:bg-gray-100"
    >
      Log In
    </Button>
  );
};

export default LoginButton;