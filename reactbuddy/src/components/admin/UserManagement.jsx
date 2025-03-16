import React, { useState, useEffect, useRef } from 'react';
import { useAdmin } from '../../hooks/useAdmin';
import Spinner from '../common/Spinner';

const UserManagement = () => {
  // Get data and functions from admin context
  const { users, fetchUsers, updateUserStatus, loading, error } = useAdmin();

  const [currentPage, setCurrentPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState('');
  const [filters, setFilters] = useState({
    sport: 'all',
    status: 'active',
    isVerified: 'false'
  });
  
  // Use ref to prevent duplicate API calls
  const initialLoadPerformed = useRef(false);

  // Load users data when component mounts
  useEffect(() => {
    // Only load data once on component mount
    if (!initialLoadPerformed.current) {
      initialLoadPerformed.current = true;
      
      // If users aren't already in the context, fetch them
      if (!users?.items?.length) {
        handleLoadData();
      }
    }
  }, []);

  // Handle loading data
  const handleLoadData = async () => {
    try {
      await fetchUsers(currentPage, 10, {
        sport: filters.sport,
        status: filters.status,
        isVerified: filters.isVerified,
        search: searchTerm || '.'
      });
    } catch (err) {
      console.error('Failed to load users:', err);
    }
  };

  // Handle search form submission
  const handleSearch = async (e) => {
    e.preventDefault();
    setCurrentPage(1);
    
    try {
      await fetchUsers(1, 10, {
        sport: filters.sport,
        status: filters.status,
        isVerified: filters.isVerified,
        search: searchTerm || '.'
      });
    } catch (err) {
      console.error('Search failed:', err);
    }
  };

  // Handle filter changes
  const handleFilterChange = (e) => {
    const { name, value } = e.target;
    setFilters(prev => ({
      ...prev,
      [name]: value
    }));
  };
  
  // Apply filters
  const handleApplyFilters = async () => {
    setCurrentPage(1);
    
    try {
      await fetchUsers(1, 10, {
        sport: filters.sport,
        status: filters.status,
        isVerified: filters.isVerified,
        search: searchTerm || '.'
      });
    } catch (err) {
      console.error('Failed to apply filters:', err);
    }
  };

  // Handle user status toggle
  const handleToggleStatus = async (userId, currentStatus) => {
    try {
      await updateUserStatus(userId, !currentStatus);
    } catch (err) {
      console.error('Error toggling user status:', err);
    }
  };

  // Handle pagination
  const handlePageChange = async (newPage) => {
    if (newPage > 0 && (!users.totalPages || newPage <= users.totalPages)) {
      setCurrentPage(newPage);
      
      try {
        await fetchUsers(newPage, 10, {
          sport: filters.sport,
          status: filters.status,
          isVerified: filters.isVerified,
          search: searchTerm || '.'
        });
      } catch (err) {
        console.error('Failed to change page:', err);
      }
    }
  };

  // Show loading spinner while initial data is being loaded
  if (loading && !users?.items?.length) {
    return (
      <div className="container mx-auto px-4 py-6">
        <div className="mb-6">
          <h1 className="text-2xl font-bold">User Management</h1>
          <p className="text-gray-600">Loading user data...</p>
        </div>
        <div className="flex justify-center items-center p-12">
          <Spinner size="lg" />
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold">User Management</h1>
        <p className="text-gray-600">
          Manage users on the BuddyUp platform.
        </p>
      </div>

      {/* Search and filters */}
      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <div className="flex flex-col md:flex-row gap-4 mb-4">
          <div className="flex-grow">
            <form onSubmit={handleSearch} className="flex gap-2">
              <input
                type="text"
                placeholder="Search users..."
                className="border rounded-md p-2 flex-grow"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
              <button 
                type="submit"
                className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
                disabled={loading}
              >
                Search
              </button>
            </form>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <select
              id="status"
              name="status"
              className="border rounded-md p-2 w-full"
              value={filters.status}
              onChange={handleFilterChange}
            >
              <option value="">All Status</option>
              <option value="active">Active</option>
              <option value="inactive">Inactive</option>
            </select>
          </div>

          <div>
            <label htmlFor="isVerified" className="block text-sm font-medium text-gray-700 mb-1">Verification</label>
            <select
              id="isVerified"
              name="isVerified"
              className="border rounded-md p-2 w-full"
              value={filters.isVerified}
              onChange={handleFilterChange}
            >
              <option value="">All Verification</option>
              <option value="true">Verified</option>
              <option value="false">Unverified</option>
            </select>
          </div>

          <div>
            <label htmlFor="sport" className="block text-sm font-medium text-gray-700 mb-1">Sport</label>
            <select
              id="sport"
              name="sport"
              className="border rounded-md p-2 w-full"
              value={filters.sport}
              onChange={handleFilterChange}
            >
              <option value="all">All Sports</option>
              <option value="tennis">Tennis</option>
              <option value="basketball">Basketball</option>
              <option value="running">Running</option>
              <option value="cycling">Cycling</option>
              <option value="swimming">Swimming</option>
            </select>
          </div>
        </div>
        
        <div className="mt-4">
          <button 
            onClick={handleApplyFilters}
            className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
            disabled={loading}
          >
            Apply Filters
          </button>
          
          <button 
            onClick={handleLoadData}
            className="ml-2 bg-gray-200 text-gray-800 px-4 py-2 rounded-md hover:bg-gray-300"
            disabled={loading}
          >
            {loading ? 'Loading...' : 'Refresh Data'}
          </button>
        </div>
      </div>

      {/* User list */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        {error && (
          <div className="bg-red-100 border-l-4 border-red-500 text-red-700 p-4 mb-4">
            <p>{error}</p>
          </div>
        )}

        {loading ? (
          <div className="flex justify-center items-center p-12">
            <Spinner size="lg" />
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Verification</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {users?.items?.length > 0 ? (
                    users.items.map((user) => (
                      <tr key={user.userId}>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="font-medium text-gray-900">{user.firstName} {user.lastName}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-gray-500">{user.email}</td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          {user.isVerified ? (
                            <span className="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800">
                              Verified
                            </span>
                          ) : (
                            <span className="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-gray-100 text-gray-800">
                              Unverified
                            </span>
                          )}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          {user.active ? (
                            <span className="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800">
                              Active
                            </span>
                          ) : (
                            <span className="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-red-100 text-red-800">
                              Inactive
                            </span>
                          )}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm">
                          <button 
                            className="text-blue-600 hover:text-blue-900 mr-4"
                            onClick={() => {/* View user details */}}
                          >
                            Details
                          </button>
                          <button 
                            className={`${user.active ? 'text-red-600 hover:text-red-900' : 'text-green-600 hover:text-green-900'}`}
                            onClick={() => handleToggleStatus(user.userId, user.active)}
                          >
                            {user.active ? 'Deactivate' : 'Activate'}
                          </button>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan="5" className="px-6 py-4 text-center text-gray-500">
                        No users found with the current filters.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {users?.totalPages > 0 && (
              <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
                <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                  <div>
                    <p className="text-sm text-gray-700">
                      Showing <span className="font-medium">{users.items.length > 0 ? ((currentPage - 1) * 10) + 1 : 0}</span> to{' '}
                      <span className="font-medium">{Math.min(currentPage * 10, users.totalItems || 0)}</span> of{' '}
                      <span className="font-medium">{users.totalItems || 0}</span> results
                    </p>
                  </div>
                  <div>
                    <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                      <button
                        onClick={() => handlePageChange(currentPage - 1)}
                        disabled={currentPage === 1 || loading}
                        className={`relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 ${
                          currentPage === 1 || loading ? 'opacity-50 cursor-not-allowed' : ''
                        }`}
                      >
                        Previous
                      </button>
                      
                      {Array.from({ length: Math.min(5, users.totalPages) }, (_, i) => {
                        const page = i + Math.max(1, currentPage - 2);
                        if (page <= users.totalPages) {
                          return (
                            <button
                              key={page}
                              onClick={() => handlePageChange(page)}
                              disabled={loading}
                              className={`relative inline-flex items-center px-4 py-2 border border-gray-300 bg-white text-sm font-medium ${
                                currentPage === page
                                  ? 'z-10 bg-blue-50 border-blue-500 text-blue-600'
                                  : 'text-gray-500 hover:bg-gray-50'
                              } ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
                            >
                              {page}
                            </button>
                          );
                        }
                        return null;
                      })}
                      
                      <button
                        onClick={() => handlePageChange(currentPage + 1)}
                        disabled={currentPage === users.totalPages || loading}
                        className={`relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 ${
                          currentPage === users.totalPages || loading ? 'opacity-50 cursor-not-allowed' : ''
                        }`}
                      >
                        Next
                      </button>
                    </nav>
                  </div>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default UserManagement;