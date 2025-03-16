import React, { useState, useEffect } from 'react';
import { Routes, Route, Link, useNavigate, useLocation } from 'react-router-dom';
import { useAdmin } from '../hooks/useAdmin';
import { useAuth } from '../hooks/useAuth';
import Spinner from '../components/common/Spinner';
import SportManagement from '../components/admin/SportManagement';
import LocationManagement from '../components/admin/LocationManagement';
import UserManagement from '../components/admin/UserManagement';
import DashboardStats from '../components/admin/DashboardStats';
import MinimalUserDisplay from '../components/admin/MinimalUserDisplay';
/**
 * Admin page component with sub-routes for different admin functions
 */
const Admin = () => {
  const { isAdmin, isLoading } = useAuth();
  const { dashboardStats, loading: adminLoading } = useAdmin();
  const navigate = useNavigate();
  const location = useLocation();
  
  // If not on a specific admin sub-route, redirect to dashboard
  useEffect(() => {
    if (location.pathname === '/admin') {
      navigate('/admin/dashboard');
    }
  }, [location, navigate]);

  // Redirect if not an admin
  useEffect(() => {
    if (!isLoading && !isAdmin) {
      navigate('/');
    }
  }, [isAdmin, isLoading, navigate]);

  if (isLoading || adminLoading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <Spinner size="lg" />
      </div>
    );
  }

  if (!isAdmin) {
    return null; // Will redirect via useEffect
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-6">Admin Panel</h1>
    
      {/* Admin Navigation */}
      <div className="bg-white shadow-md rounded-lg p-4 mb-8">
        <nav className="flex flex-wrap gap-2">
          <Link
            to="/admin/dashboard"
            className={`px-4 py-2 rounded-md ${
              location.pathname === '/admin/dashboard'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
            }`}
          >
            Dashboard
          </Link>
          <Link
            to="/admin/users"
            className={`px-4 py-2 rounded-md ${
              location.pathname === '/admin/users'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
            }`}
          >
            Users
          </Link>
          <Link
            to="/admin/sports"
            className={`px-4 py-2 rounded-md ${
              location.pathname === '/admin/sports'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
            }`}
          >
            Sports
          </Link>
          <Link
            to="/admin/locations"
            className={`px-4 py-2 rounded-md ${
              location.pathname === '/admin/locations'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
            }`}
          >
            Locations
          </Link>
          <Link
            to="/admin/reports"
            className={`px-4 py-2 rounded-md ${
              location.pathname === '/admin/reports'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
            }`}
          >
            Reports
          </Link>
          <Link
            to="/admin/verifications"
            className={`px-4 py-2 rounded-md ${
              location.pathname === '/admin/verifications'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
            }`}
          >
            Verifications
          </Link>
        </nav>
      </div>

      {/* Sub-routes for different admin functions */}
      <Routes>
        <Route path="dashboard" element={<DashboardStats stats={dashboardStats} />} />
        <Route path="users" element={<UserManagement />} />
        <Route path="sports" element={<SportManagement />} />
        <Route path="locations" element={<LocationManagement />} />
        <Route path="reports" element={
          <div className="bg-white shadow-md rounded-lg p-6">
            <h2 className="text-2xl font-semibold mb-4">User Reports</h2>
            <p className="text-gray-500 italic mb-4">
              This feature will be implemented in a future update.
            </p>
            
            <div className="border border-gray-200 rounded-md p-4">
              <div className="flex items-center justify-between mb-4">
                <div>
                  <h3 className="font-medium">Filter Reports</h3>
                </div>
                <select className="border rounded-md p-2 text-sm" disabled>
                  <option>All Reports</option>
                  <option>Pending</option>
                  <option>Reviewed</option>
                  <option>Dismissed</option>
                </select>
              </div>
              
              <div className="opacity-50 pointer-events-none">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead>
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Reported User</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Reported By</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Reason</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    <tr>
                      <td className="px-6 py-4 whitespace-nowrap">Sample User</td>
                      <td className="px-6 py-4 whitespace-nowrap">Reporting User</td>
                      <td className="px-6 py-4">Inappropriate behavior during workout</td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className="px-2 py-1 text-xs rounded-full bg-yellow-100 text-yellow-800">Pending</span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">2023-01-01</td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <button className="text-blue-600 hover:text-blue-800 mr-2">Review</button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        } />
        
        <Route path="verifications" element={
          <div className="bg-white shadow-md rounded-lg p-6">
            <h2 className="text-2xl font-semibold mb-4">Verification Requests</h2>
            <p className="text-gray-500 italic mb-4">
              This feature will be implemented in a future update.
            </p>
            
            <div className="border border-gray-200 rounded-md p-4">
              <div className="flex items-center justify-between mb-4">
                <div>
                  <h3 className="font-medium">Filter Verifications</h3>
                </div>
                <select className="border rounded-md p-2 text-sm" disabled>
                  <option>All Requests</option>
                  <option>Pending</option>
                  <option>Approved</option>
                  <option>Rejected</option>
                </select>
              </div>
              
              <div className="opacity-50 pointer-events-none">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead>
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">User</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Provider</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Submitted</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    <tr>
                      <td className="px-6 py-4 whitespace-nowrap">Sample User</td>
                      <td className="px-6 py-4 whitespace-nowrap">Identity</td>
                      <td className="px-6 py-4 whitespace-nowrap">CLEAR</td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className="px-2 py-1 text-xs rounded-full bg-yellow-100 text-yellow-800">Pending</span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">2023-01-01</td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <button className="text-green-600 hover:text-green-800 mr-2">Approve</button>
                        <button className="text-red-600 hover:text-red-800">Reject</button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        } />
      </Routes>
    </div>
  );
};

export default Admin;