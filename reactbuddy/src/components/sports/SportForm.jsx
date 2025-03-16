import React, { useState, useEffect } from 'react';
import { useSports } from '../../context/SportsContext';

const SportForm = () => {
  const { selectedSport, addSport, updateSport, selectSport } = useSports();
  
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    iconUrl: '',
    isActive: true
  });
  
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [apiError, setApiError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);
  
  // Reset form when selected sport changes
  useEffect(() => {
    if (selectedSport) {
      setFormData({
        name: selectedSport.name || '',
        description: selectedSport.description || '',
        iconUrl: selectedSport.iconUrl || '',
        isActive: selectedSport.isActive
      });
    } else {
      setFormData({
        name: '',
        description: '',
        iconUrl: '',
        isActive: true
      });
    }
    
    setErrors({});
    setApiError(null);
    setSuccessMessage(null);
  }, [selectedSport]);
  
  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.name.trim()) {
      newErrors.name = 'Sport name is required';
    } else if (formData.name.length > 50) {
      newErrors.name = 'Sport name must be less than 50 characters';
    }
    
    if (formData.description && formData.description.length > 500) {
      newErrors.description = 'Description must be less than 500 characters';
    }
    
    if (formData.iconUrl && !isValidUrl(formData.iconUrl)) {
      newErrors.iconUrl = 'Please enter a valid URL';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };
  
  const isValidUrl = (string) => {
    try {
      new URL(string);
      return true;
    } catch (_) {
      return false;
    }
  };
  
  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? checked : value
    });
    
    // Clear specific error when field is changed
    if (errors[name]) {
      setErrors({
        ...errors,
        [name]: undefined
      });
    }
    
    // Clear success/error messages when form is modified
    setSuccessMessage(null);
    setApiError(null);
  };
  
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }
    
    setSubmitting(true);
    setApiError(null);
    
    try {
      if (selectedSport) {
        // Update existing sport
        await updateSport(selectedSport.sportId, formData);
        setSuccessMessage(`Sport "${formData.name}" has been updated successfully.`);
      } else {
        // Create new sport
        await addSport(formData);
        setSuccessMessage(`Sport "${formData.name}" has been added successfully.`);
        // Reset form after successful creation
        setFormData({
          name: '',
          description: '',
          iconUrl: '',
          isActive: true
        });
      }
    } catch (error) {
      setApiError(error.response?.data?.message || error.message || 'An error occurred');
    } finally {
      setSubmitting(false);
    }
  };
  
  const handleCancel = () => {
    selectSport(null);
  };
  
  return (
    <div className="bg-white rounded-lg shadow-md mb-4 overflow-hidden">
      <div className="bg-gray-100 px-4 py-3 border-b">
        <h5 className="text-lg font-semibold">
          {selectedSport ? `Edit Sport: ${selectedSport.name}` : 'Add New Sport'}
        </h5>
      </div>
      <div className="p-4">
        {successMessage && (
          <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-4 relative" role="alert">
            <span className="block sm:inline">{successMessage}</span>
            <button
              type="button"
              className="absolute top-0 right-0 p-1.5 mt-0.5 mr-0.5 text-green-500 hover:text-green-800"
              onClick={() => setSuccessMessage(null)}
            >
              <span className="sr-only">Close</span>
              <svg className="h-4 w-4" fill="currentColor" viewBox="0 0 20 20">
                <path
                  fillRule="evenodd"
                  d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                  clipRule="evenodd"
                />
              </svg>
            </button>
          </div>
        )}
        
        {apiError && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4 relative" role="alert">
            <span className="block sm:inline">{apiError}</span>
            <button
              type="button"
              className="absolute top-0 right-0 p-1.5 mt-0.5 mr-0.5 text-red-500 hover:text-red-800"
              onClick={() => setApiError(null)}
            >
              <span className="sr-only">Close</span>
              <svg className="h-4 w-4" fill="currentColor" viewBox="0 0 20 20">
                <path
                  fillRule="evenodd"
                  d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                  clipRule="evenodd"
                />
              </svg>
            </button>
          </div>
        )}
        
        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label htmlFor="sportName" className="block text-gray-700 font-medium mb-1">
              Sport Name *
            </label>
            <input
              type="text"
              id="sportName"
              name="name"
              value={formData.name}
              onChange={handleChange}
              className={`w-full px-3 py-2 border rounded-md ${errors.name ? 'border-red-500' : 'border-gray-300'}`}
              placeholder="Enter sport name"
              required
            />
            {errors.name && (
              <p className="text-red-500 text-sm mt-1">{errors.name}</p>
            )}
          </div>
          
          <div className="mb-4">
            <label htmlFor="sportDescription" className="block text-gray-700 font-medium mb-1">
              Description
            </label>
            <textarea
              id="sportDescription"
              name="description"
              value={formData.description}
              onChange={handleChange}
              className={`w-full px-3 py-2 border rounded-md ${errors.description ? 'border-red-500' : 'border-gray-300'}`}
              placeholder="Enter sport description"
              rows={3}
            />
            {errors.description && (
              <p className="text-red-500 text-sm mt-1">{errors.description}</p>
            )}
          </div>
          
          <div className="mb-4">
            <label htmlFor="sportIconUrl" className="block text-gray-700 font-medium mb-1">
              Icon URL
            </label>
            <input
              type="text"
              id="sportIconUrl"
              name="iconUrl"
              value={formData.iconUrl}
              onChange={handleChange}
              className={`w-full px-3 py-2 border rounded-md ${errors.iconUrl ? 'border-red-500' : 'border-gray-300'}`}
              placeholder="Enter icon URL"
            />
            {errors.iconUrl && (
              <p className="text-red-500 text-sm mt-1">{errors.iconUrl}</p>
            )}
            <p className="text-gray-500 text-sm mt-1">
              URL to an image that represents this sport
            </p>
          </div>
          
          {selectedSport && (
            <div className="mb-4">
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="sportIsActive"
                  name="isActive"
                  checked={formData.isActive}
                  onChange={handleChange}
                  className="h-4 w-4 text-blue-600 border-gray-300 rounded"
                />
                <label htmlFor="sportIsActive" className="ml-2 block text-gray-700">
                  Active
                </label>
              </div>
              <p className="text-gray-500 text-sm mt-1">
                Inactive sports won't be shown to users
              </p>
            </div>
          )}
          
          <div className="flex justify-end">
            {selectedSport && (
              <button 
                type="button"
                onClick={handleCancel}
                className="mr-2 px-4 py-2 border border-gray-300 rounded-md shadow-sm text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                disabled={submitting}
              >
                Cancel
              </button>
            )}
            <button 
              type="submit"
              className="px-4 py-2 border border-transparent rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              disabled={submitting}
            >
              {submitting ? 'Saving...' : (selectedSport ? 'Update Sport' : 'Add Sport')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default SportForm;