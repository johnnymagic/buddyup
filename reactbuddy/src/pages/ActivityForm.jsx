import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useActivity } from '../context/ActivityContext';
import { useProfile } from '../context/ProfileContext';
import { useAuth } from '../context/AuthContext';

const ActivityForm = () => {
  const { activityId } = useParams();
  const navigate = useNavigate();
  const { getActivityById, createActivity, updateActivity, loading, error } = useActivity();
  const { availableSports } = useProfile();
  const { userProfile } = useAuth();
  
  const isEditMode = !!activityId;
  
  // Default form state
  const defaultFormState = {
    name: '',
    sportId: '',
    locationId: userProfile?.locationId || null,
    description: '',
    difficultyLevel: 'beginner',
    maxParticipants: 10,
    recurringSchedule: '',
    isActive: true
  };
  
  const [formData, setFormData] = useState(defaultFormState);
  const [formErrors, setFormErrors] = useState({});
  const [submitError, setSubmitError] = useState(null);

  // Fetch activity data if in edit mode
  useEffect(() => {
    const fetchActivityData = async () => {
      if (isEditMode) {
        try {
          const activity = await getActivityById(activityId);
          
          if (activity) {
            // Format the data for the form - using camelCase for frontend form
            setFormData({
              name: activity.name || '',
              sportId: activity.sportId || '',
              locationId: activity.locationId || null,
              description: activity.description || '',
              difficultyLevel: activity.difficultyLevel || 'beginner',
              maxParticipants: activity.maxParticipants || 10,
              recurringSchedule: activity.recurringSchedule || '',
              isActive: activity.isActive ?? true
            });
          }
        } catch (err) {
          console.error('Failed to fetch activity for editing', err);
          setSubmitError('Failed to load activity data. Please try again.');
        }
      }
    };
    
    fetchActivityData();
  }, [activityId, getActivityById, isEditMode]);

  // Handle form input changes
  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    
    // Handle different input types
    const inputValue = type === 'checkbox' ? checked : value;
    
    // Special handling for number inputs
    const parsedValue = 
      (name === 'maxParticipants' && value !== '') 
        ? parseInt(value, 10) 
        : inputValue;
    
    setFormData(prev => ({
      ...prev,
      [name]: parsedValue
    }));
    
    // Clear specific field error when user makes a change
    if (formErrors[name]) {
      setFormErrors(prev => ({
        ...prev,
        [name]: null
      }));
    }
  };

  // Validate form data
  const validateForm = () => {
    const errors = {};
    
    if (!formData.name.trim()) {
      errors.name = 'Activity name is required';
    }
    
    if (!formData.sportId) {
      errors.sportId = 'Please select a sport';
    }
    
    if (!formData.description.trim()) {
      errors.description = 'Description is required';
    }
    
    if (formData.maxParticipants && (formData.maxParticipants < 1 || formData.maxParticipants > 100)) {
      errors.maxParticipants = 'Participants must be between 1 and 100';
    }
    
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // Reset submission error
    setSubmitError(null);
    
    // Validate form
    const isValid = validateForm();
    
    if (!isValid) {
      return;
    }
    
    try {
      if (isEditMode) {
        // Pass formData directly - the context will handle PascalCase conversion
        await updateActivity(activityId, formData);
        navigate(`/activity/${activityId}`);
      } else {
        // Pass formData directly - the context will handle PascalCase conversion
        const newActivity = await createActivity(formData);
        navigate(`/activity/${newActivity.activityId}`);
      }
    } catch (err) {
      console.error('Failed to save activity', err);
      setSubmitError('Failed to save activity. Please try again.');
    }
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-6">
        <Link to={isEditMode ? `/activity/${activityId}` : "/activities"} className="text-blue-600 hover:text-blue-800">
          &larr; {isEditMode ? 'Back to Activity' : 'Back to Activities'}
        </Link>
      </div>

      <div className="bg-white rounded-lg shadow-lg overflow-hidden mb-8">
        <div className="p-6">
          <h1 className="text-3xl font-bold text-gray-900 mb-6">
            {isEditMode ? 'Edit Activity' : 'Create New Activity'}
          </h1>

          {submitError && (
            <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-6">
              <div className="flex">
                <div className="flex-shrink-0">
                  <svg className="h-5 w-5 text-red-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <p className="text-sm text-red-700">{submitError}</p>
                </div>
              </div>
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
              {/* Activity Name */}
              <div>
                <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
                  Activity Name *
                </label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleChange}
                  className={`w-full p-2 border ${formErrors.name ? 'border-red-500' : 'border-gray-300'} rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500`}
                  placeholder="e.g., Weekend Basketball Game"
                />
                {formErrors.name && <p className="mt-1 text-sm text-red-600">{formErrors.name}</p>}
              </div>

              {/* Sport Selection */}
              <div>
                <label htmlFor="sportId" className="block text-sm font-medium text-gray-700 mb-1">
                  Sport *
                </label>
                <select
                  id="sportId"
                  name="sportId"
                  value={formData.sportId}
                  onChange={handleChange}
                  className={`w-full p-2 border ${formErrors.sportId ? 'border-red-500' : 'border-gray-300'} rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500`}
                >
                  <option value="">Select a Sport</option>
                  {availableSports.map(sport => (
                    <option key={sport.sportId} value={sport.sportId}>
                      {sport.name}
                    </option>
                  ))}
                </select>
                {formErrors.sportId && <p className="mt-1 text-sm text-red-600">{formErrors.sportId}</p>}
              </div>

              {/* Difficulty Level */}
              <div>
                <label htmlFor="difficultyLevel" className="block text-sm font-medium text-gray-700 mb-1">
                  Difficulty Level
                </label>
                <select
                  id="difficultyLevel"
                  name="difficultyLevel"
                  value={formData.difficultyLevel}
                  onChange={handleChange}
                  className="w-full p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="beginner">Beginner</option>
                  <option value="intermediate">Intermediate</option>
                  <option value="advanced">Advanced</option>
                  <option value="expert">Expert</option>
                </select>
              </div>

              {/* Max Participants */}
              <div>
                <label htmlFor="maxParticipants" className="block text-sm font-medium text-gray-700 mb-1">
                  Maximum Participants
                </label>
                <input
                  type="number"
                  id="maxParticipants"
                  name="maxParticipants"
                  value={formData.maxParticipants}
                  onChange={handleChange}
                  min="1"
                  max="100"
                  className={`w-full p-2 border ${formErrors.maxParticipants ? 'border-red-500' : 'border-gray-300'} rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500`}
                />
                {formErrors.maxParticipants && <p className="mt-1 text-sm text-red-600">{formErrors.maxParticipants}</p>}
              </div>

              {/* Recurring Schedule */}
              <div className="md:col-span-2">
                <label htmlFor="recurringSchedule" className="block text-sm font-medium text-gray-700 mb-1">
                  Schedule (Optional)
                </label>
                <input
                  type="text"
                  id="recurringSchedule"
                  name="recurringSchedule"
                  value={formData.recurringSchedule}
                  onChange={handleChange}
                  className="w-full p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="e.g., Every Saturday at 10:00 AM"
                />
              </div>

              {/* Description */}
              <div className="md:col-span-2">
                <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
                  Description *
                </label>
                <textarea
                  id="description"
                  name="description"
                  value={formData.description}
                  onChange={handleChange}
                  rows="4"
                  className={`w-full p-2 border ${formErrors.description ? 'border-red-500' : 'border-gray-300'} rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500`}
                  placeholder="Describe the activity, what to expect, what to bring, etc."
                ></textarea>
                {formErrors.description && <p className="mt-1 text-sm text-red-600">{formErrors.description}</p>}
              </div>

              {/* Active Status (for edit mode) */}
              {isEditMode && (
                <div className="md:col-span-2">
                  <div className="flex items-center">
                    <input
                      type="checkbox"
                      id="isActive"
                      name="isActive"
                      checked={formData.isActive}
                      onChange={handleChange}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <label htmlFor="isActive" className="ml-2 block text-sm text-gray-700">
                      Activity is active and visible to other users
                    </label>
                  </div>
                </div>
              )}
            </div>

            <div className="flex justify-end space-x-4">
              <Link
                to={isEditMode ? `/activity/${activityId}` : "/activities"}
                className="px-6 py-2 border border-gray-300 text-gray-700 font-medium rounded-md hover:bg-gray-50 transition-colors"
              >
                Cancel
              </Link>
              <button
                type="submit"
                className="px-6 py-2 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                disabled={loading}
              >
                {loading ? (
                  <span className="flex items-center">
                    <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Saving...
                  </span>
                ) : (
                  isEditMode ? 'Update Activity' : 'Create Activity'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default ActivityForm;