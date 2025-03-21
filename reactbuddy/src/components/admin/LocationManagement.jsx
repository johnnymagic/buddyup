import React, { useState, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { useAdmin } from '../../hooks/useAdmin';
import Button from '../common/Button';
import Spinner from '../common/Spinner';

// Modal component using React Portal
const Modal = ({ isOpen, onClose, title, children }) => {
  // Don't render anything if the modal is not open
  if (!isOpen) return null;

  // Create a portal to render the modal outside the React tree
  return createPortal(
    <div 
      className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      style={{ backdropFilter: 'blur(2px)' }}
    >
      <div className="bg-white rounded-lg shadow-lg w-full max-w-lg overflow-hidden">
        <div className="p-4 border-b flex justify-between items-center">
          <h3 className="text-lg font-medium">{title}</h3>
          <button 
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600"
          >
            &times;
          </button>
        </div>
        <div className="overflow-y-auto">
          {children}
        </div>
      </div>
    </div>,
    document.body
  );
};

/**
 * Location Management component for admin dashboard
 */
const LocationManagement = () => {
  const { locations, fetchLocations, createLocation, loading, error } = useAdmin();
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [formData, setFormData] = useState({
    name: '',
    address: '',
    city: '',
    state: '',
    postalCode: '',
    country: '',
    latitude: '',
    longitude: ''
  });
  
  // Debug log for locations data
  console.log('LocationManagement - locations data:', locations);
  
  // Manual load function
  const handleLoadData = async () => {
    console.log('Attempting to load locations data...');
    try {
      const response = await fetchLocations(currentPage, 10);
      console.log('Location data fetched:', response);
    } catch (err) {
      console.error('Error loading locations:', err);
    }
  };
  
  console.log('Render state - loading:', loading, 'locations items length:', locations?.items?.length);
  
  const openModal = () => {
    console.log('Opening modal');
    setFormData({
      name: '',
      address: '',
      city: '',
      state: '',
      postalCode: '',
      country: '',
      latitude: '',
      longitude: ''
    });
    setIsModalOpen(true);
  };
  
  const closeModal = () => {
    console.log('Closing modal');
    setIsModalOpen(false);
  };
  
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value
    }));
  };
  
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // Validate coordinates
    const lat = parseFloat(formData.latitude);
    const lng = parseFloat(formData.longitude);
    
    if (isNaN(lat) || lat < -90 || lat > 90) {
      alert('Latitude must be a number between -90 and 90');
      return;
    }
    
    if (isNaN(lng) || lng < -180 || lng > 180) {
      alert('Longitude must be a number between -180 and 180');
      return;
    }
    
    try {
      await createLocation({
        name: formData.name,
        address: formData.address,
        city: formData.city,
        state: formData.state,
        zipCode: formData.postalCode,
        country: formData.country,
        latitude: lat,
        longitude: lng,
        isActive: true
      });
      
      closeModal();
      // Manually refresh the list after adding
      fetchLocations(currentPage, 10);
    } catch (err) {
      console.error('Error saving location:', err);
    }
  };
  
  const handlePageChange = async (newPage) => {
    if (newPage > 0 && (!locations.totalPages || newPage <= locations.totalPages)) {
      setCurrentPage(newPage);
      
      try {
        await fetchLocations(newPage, 10);
      } catch (err) {
        console.error('Page change failed:', err);
      }
    }
  };
  
  // If data hasn't been loaded yet, show the load button
  if (!locations?.items?.length) {
    return (
      <div className="bg-white shadow-md rounded-lg p-6">
        <div className="mb-6">
          <h2 className="text-2xl font-semibold">Location Management</h2>
          <p className="text-gray-600">Click the button below to load location data.</p>
        </div>
        
        {/* Debug info - can be removed in production */}
        <div className="mb-4 p-2 bg-gray-100 text-xs">
          <div>Debug - Items Count: {locations?.items?.length || 0}</div>
          <div>Debug - Total Items: {locations?.totalItems || 0}</div>
          <div>Debug - Loading: {loading ? 'Yes' : 'No'}</div>
        </div>
        
        <div className="flex space-x-3">
          <Button
            onClick={handleLoadData}
            disabled={loading}
            className="bg-blue-600 hover:bg-blue-700 text-white"
          >
            {loading ? 'Loading...' : 'Load Locations'}
          </Button>
          
          <Button
            onClick={openModal}
            disabled={loading}
            className="bg-green-600 hover:bg-green-700 text-white"
          >
            Add New Location
          </Button>
        </div>
        
        {error && (
          <div className="mt-4 bg-red-50 border-l-4 border-red-400 p-4">
            <p className="text-sm text-red-700">{error}</p>
          </div>
        )}
        
        {/* Modal for adding locations */}
        <Modal isOpen={isModalOpen} onClose={closeModal} title="Add New Location">
          <form onSubmit={handleSubmit}>
            <div className="p-4">
              <div className="mb-4">
                <label htmlFor="name" className="block text-sm font-medium text-gray-700">
                  Location Name *
                </label>
                <input
                  type="text"
                  name="name"
                  id="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  required
                  className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
              
              <div className="mb-4">
                <label htmlFor="address" className="block text-sm font-medium text-gray-700">
                  Address
                </label>
                <textarea
                  name="address"
                  id="address"
                  rows="2"
                  value={formData.address}
                  onChange={handleInputChange}
                  className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                ></textarea>
              </div>
              
              <div className="grid grid-cols-2 gap-4 mb-4">
                <div>
                  <label htmlFor="city" className="block text-sm font-medium text-gray-700">
                    City *
                  </label>
                  <input
                    type="text"
                    name="city"
                    id="city"
                    value={formData.city}
                    onChange={handleInputChange}
                    required
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  />
                </div>
                <div>
                  <label htmlFor="state" className="block text-sm font-medium text-gray-700">
                    State/Province *
                  </label>
                  <input
                    type="text"
                    name="state"
                    id="state"
                    value={formData.state}
                    onChange={handleInputChange}
                    required
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  />
                </div>
              </div>
              
              <div className="grid grid-cols-2 gap-4 mb-4">
                <div>
                  <label htmlFor="postalCode" className="block text-sm font-medium text-gray-700">
                    Postal Code
                  </label>
                  <input
                    type="text"
                    name="postalCode"
                    id="postalCode"
                    value={formData.postalCode}
                    onChange={handleInputChange}
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  />
                </div>
                <div>
                  <label htmlFor="country" className="block text-sm font-medium text-gray-700">
                    Country *
                  </label>
                  <input
                    type="text"
                    name="country"
                    id="country"
                    value={formData.country}
                    onChange={handleInputChange}
                    required
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  />
                </div>
              </div>
              
              <div className="grid grid-cols-2 gap-4 mb-4">
                <div>
                  <label htmlFor="latitude" className="block text-sm font-medium text-gray-700">
                    Latitude *
                  </label>
                  <input
                    type="text"
                    name="latitude"
                    id="latitude"
                    value={formData.latitude}
                    onChange={handleInputChange}
                    required
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  />
                </div>
                <div>
                  <label htmlFor="longitude" className="block text-sm font-medium text-gray-700">
                    Longitude *
                  </label>
                  <input
                    type="text"
                    name="longitude"
                    id="longitude"
                    value={formData.longitude}
                    onChange={handleInputChange}
                    required
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  />
                </div>
              </div>
              <p className="text-xs text-gray-500 mt-1">
                * Required fields. Latitude must be between -90 and 90, longitude must be between -180 and 180.
              </p>
            </div>
            
            <div className="bg-gray-50 px-4 py-3 flex justify-end space-x-3 border-t">
              <button
                type="button"
                className="inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                onClick={closeModal}
              >
                Cancel
              </button>
              <button
                type="submit"
                className="inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                disabled={loading}
              >
                {loading ? 'Saving...' : 'Add Location'}
              </button>
            </div>
          </form>
        </Modal>
      </div>
    );
  }

  return (
    <div className="bg-white shadow-md rounded-lg p-6">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-semibold">Location Management</h2>
        <div className="flex space-x-2">
          <Button
            onClick={handleLoadData}
            className="bg-gray-200 hover:bg-gray-300 text-gray-800"
            disabled={loading}
          >
            {loading ? 'Refreshing...' : 'Refresh Data'}
          </Button>
          <Button
            onClick={openModal}
            className="bg-blue-600 hover:bg-blue-700 text-white"
            disabled={loading}
          >
            Add New Location
          </Button>
        </div>
      </div>
      
      {/* Debug info - can be removed in production */}
      <div className="mb-4 p-2 bg-gray-100 text-xs">
        <div>Debug - Items Count: {locations?.items?.length || 0}</div>
        <div>Debug - Total Items: {locations?.totalItems || 0}</div>
        <div>Debug - Loading: {loading ? 'Yes' : 'No'}</div>
      </div>
      
      {error && (
        <div className="mb-4 bg-red-50 border-l-4 border-red-400 p-4">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg className="h-5 w-5 text-red-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm text-red-700">{error}</p>
            </div>
          </div>
        </div>
      )}
      
      {loading && !locations?.items?.length ? (
        <div className="flex justify-center py-12">
          <Spinner size="lg" />
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Name
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Address
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  City
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Coordinates
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {locations?.items?.length > 0 ? (
                locations.items.map((location) => (
                  <tr key={location.locationId}>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">{location.name}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-normal">
                      <div className="text-sm text-gray-500 max-w-md">
                        {location.address}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-500">
                        {location.city}, {location.state}
                        {location.country && `, ${location.country}`}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-500">
                        {location.latitude?.toFixed(6)}, {location.longitude?.toFixed(6)}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full ${
                        location.isActive
                          ? 'bg-green-100 text-green-800'
                          : 'bg-gray-100 text-gray-800'
                      }`}>
                        {location.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5} className="px-6 py-4 text-center text-sm text-gray-500">
                    No locations found. Click "Add New Location" to create one.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
          
          {/* Pagination */}
          {locations?.totalPages > 1 && (
            <div className="flex items-center justify-between px-4 py-3 bg-white border-t border-gray-200 sm:px-6">
              <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
                <div>
                  <p className="text-sm text-gray-700">
                    Showing <span className="font-medium">{locations.items.length > 0 ? ((currentPage - 1) * 10) + 1 : 0}</span> to{' '}
                    <span className="font-medium">
                      {Math.min(currentPage * 10, locations.totalItems || 0)}
                    </span>{' '}
                    of <span className="font-medium">{locations.totalItems || 0}</span> results
                  </p>
                </div>
                <div>
                  <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                    <button
                      onClick={() => handlePageChange(currentPage - 1)}
                      disabled={currentPage === 1 || loading}
                      className={`relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium ${
                        currentPage === 1 || loading ? 'text-gray-300 cursor-not-allowed' : 'text-gray-500 hover:bg-gray-50'
                      }`}
                    >
                      <span className="sr-only">Previous</span>
                      <svg className="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                        <path fillRule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" clipRule="evenodd" />
                      </svg>
                    </button>
                    
                    {/* Show up to 5 page buttons */}
                    {[...Array(Math.min(5, locations.totalPages || 1)).keys()].map((i) => {
                      const page = i + Math.max(1, Math.min(currentPage - 2, (locations.totalPages || 1) - 4));
                      return (
                        <button
                          key={page}
                          onClick={() => handlePageChange(page)}
                          disabled={loading}
                          className={`relative inline-flex items-center px-4 py-2 border text-sm font-medium ${
                            currentPage === page
                              ? 'z-10 bg-blue-50 border-blue-500 text-blue-600'
                              : 'bg-white border-gray-300 text-gray-500 hover:bg-gray-50'
                          } ${loading ? 'cursor-not-allowed' : ''}`}
                        >
                          {page}
                        </button>
                      );
                    })}
                    
                    <button
                      onClick={() => handlePageChange(currentPage + 1)}
                      disabled={currentPage === locations.totalPages || loading}
                      className={`relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium ${
                        currentPage === locations.totalPages || loading ? 'text-gray-300 cursor-not-allowed' : 'text-gray-500 hover:bg-gray-50'
                      }`}
                    >
                      <span className="sr-only">Next</span>
                      <svg className="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                        <path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd" />
                      </svg>
                    </button>
                  </nav>
                </div>
              </div>
            </div>
          )}
        </div>
      )}
      
      {/* Modal for adding locations */}
      <Modal isOpen={isModalOpen} onClose={closeModal} title="Add New Location">
        <form onSubmit={handleSubmit}>
          <div className="p-4">
            <div className="mb-4">
              <label htmlFor="name" className="block text-sm font-medium text-gray-700">
                Location Name *
              </label>
              <input
                type="text"
                name="name"
                id="name"
                value={formData.name}
                onChange={handleInputChange}
                required
                className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              />
            </div>
            
            <div className="mb-4">
              <label htmlFor="address" className="block text-sm font-medium text-gray-700">
                Address
              </label>
              <textarea
                name="address"
                id="address"
                rows="2"
                value={formData.address}
                onChange={handleInputChange}
                className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              ></textarea>
            </div>
            
            <div className="grid grid-cols-2 gap-4 mb-4">
              <div>
                <label htmlFor="city" className="block text-sm font-medium text-gray-700">
                  City *
                </label>
                <input
                  type="text"
                  name="city"
                  id="city"
                  value={formData.city}
                  onChange={handleInputChange}
                  required
                  className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
              <div>
                <label htmlFor="state" className="block text-sm font-medium text-gray-700">
                  State/Province *
                </label>
                <input
                  type="text"
                  name="state"
                  id="state"
                  value={formData.state}
                  onChange={handleInputChange}
                  required
                  className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
            </div>
            
            <div className="grid grid-cols-2 gap-4 mb-4">
              <div>
                <label htmlFor="postalCode" className="block text-sm font-medium text-gray-700">
                  Postal Code
                </label>
                <input
                  type="text"
                  name="postalCode"
                  id="postalCode"
                  value={formData.postalCode}
                  onChange={handleInputChange}
                  className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
              <div>
                <label htmlFor="country" className="block text-sm font-medium text-gray-700">
                  Country *
                </label>
                <input
                  type="text"
                  name="country"
                  id="country"
                  value={formData.country}
                  onChange={handleInputChange}
                  required
                  className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
            </div>
            
            <div className="grid grid-cols-2 gap-4 mb-4">
              <div>
                <label htmlFor="latitude" className="block text-sm font-medium text-gray-700">
                  Latitude *
                </label>
                <input
                  type="text"
                  name="latitude"
                  id="latitude"
                  value={formData.latitude}
                  onChange={handleInputChange}
                  required
                  className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
              <div>
                <label htmlFor="longitude" className="block text-sm font-medium text-gray-700">
                  Longitude *
                </label>
                <input
                  type="text"
                  name="longitude"
                  id="longitude"
                  value={formData.longitude}
                  onChange={handleInputChange}
                  required
                  className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
            </div>
            <p className="text-xs text-gray-500 mt-1">
              * Required fields. Latitude must be between -90 and 90, longitude must be between -180 and 180.
            </p>
          </div>
          
          <div className="bg-gray-50 px-4 py-3 flex justify-end space-x-3 border-t">
            <button
              type="button"
              className="inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              onClick={closeModal}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              disabled={loading}
            >
              {loading ? 'Saving...' : 'Add Location'}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default LocationManagement;