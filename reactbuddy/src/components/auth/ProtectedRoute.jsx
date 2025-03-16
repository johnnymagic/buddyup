import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

/**
 * Protected route component that ensures user is authenticated
 * before accessing a route. Redirects to login page if not authenticated.
 * 
 * @param {Object} props - Component props
 * @param {boolean} props.adminOnly - Whether the route is admin-only
 * @param {boolean} props.verifiedOnly - Whether the route is for verified users only
 * @param {React.ReactNode} props.children - Child components to render if authorized
 */
const ProtectedRoute = ({ 
  children, 
  adminOnly = false, 
  verifiedOnly = false 
}) => {
  const { 
    isLoading, 
    isAuthenticated, 
    isAdmin, 
    isVerified, 
    hasProfile 
  } = useAuth();

  // Show loading state while authentication is being checked
  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  // Check if user is authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Check if admin route access is allowed
  if (adminOnly && !isAdmin) {
    return <Navigate to="/" replace />;
  }

  // Check if verified-only route access is allowed
  if (verifiedOnly && !isVerified) {
    return <Navigate to="/verification" replace />;
  }

  // If all checks pass, render the children
  return children;
};

export default ProtectedRoute;