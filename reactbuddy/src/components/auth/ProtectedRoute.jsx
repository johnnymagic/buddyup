import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import Spinner from '../common/Spinner';

/**
 * Protected route component that requires authentication
 * 
 * @param {Object} props - Component props
 * @param {React.ReactNode} props.children - Child components to render when authenticated
 * @param {boolean} [props.adminOnly=false] - Whether the route requires admin privileges
 */
const ProtectedRoute = ({ children, adminOnly = false }) => {
  const { isAuthenticated, isLoading, isAdmin } = useAuth();

  // Show spinner while loading authentication state
  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <Spinner size="lg" />
      </div>
    );
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }

  // Redirect to home if not admin but route requires admin
  if (adminOnly && !isAdmin) {
    return <Navigate to="/" />;
  }

  // User is authenticated (and is admin if required), show the protected content
  return children;
};

export default ProtectedRoute;