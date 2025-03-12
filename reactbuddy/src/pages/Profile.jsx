import React, { useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import ProfileForm  from '../components/profile/ProfileForm';
import VerificationStatus from '../components/profile/VerificationStatus';

/**
 * Profile page component for managing user profile
 */
const Profile = () => {
  const { userProfile, isLoading } = useAuth();
  const [activeTab, setActiveTab] = useState('profile');

  const handleTabChange = (tab) => {
    setActiveTab(tab);
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="loader">Loading...</div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-6">Your Profile</h1>
      
      {/* Tabs navigation */}
      <div className="mb-6 border-b border-gray-200">
        <nav className="flex -mb-px space-x-8">
          <button
            className={`pb-4 px-1 ${
              activeTab === 'profile'
                ? 'border-b-2 border-blue-500 text-blue-600 font-medium'
                : 'border-b-2 border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
            onClick={() => handleTabChange('profile')}
          >
            Profile Information
          </button>
          <button
            className={`pb-4 px-1 ${
              activeTab === 'verification'
                ? 'border-b-2 border-blue-500 text-blue-600 font-medium'
                : 'border-b-2 border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
            onClick={() => handleTabChange('verification')}
          >
            Verification
          </button>
          <button
            className={`pb-4 px-1 ${
              activeTab === 'settings'
                ? 'border-b-2 border-blue-500 text-blue-600 font-medium'
                : 'border-b-2 border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
            onClick={() => handleTabChange('settings')}
          >
            Settings
          </button>
        </nav>
      </div>
      
      {/* Tab content */}
      <div className="mt-6">
        {activeTab === 'profile' && (
          <ProfileForm />
        )}
        
        {activeTab === 'verification' && (
          <VerificationStatus />
        )}
        
        {activeTab === 'settings' && (
          <div className="bg-white shadow rounded-lg p-6">
            <h2 className="text-xl font-semibold mb-4">Account Settings</h2>
            <p className="text-gray-500 italic">
              Account settings will be available in a future update.
            </p>
            
            {/* Placeholder for future settings */}
            <div className="mt-6 space-y-4 opacity-50 pointer-events-none">
              <div>
                <h3 className="font-medium">Email Notifications</h3>
                <div className="mt-2">
                  <label className="inline-flex items-center">
                    <input type="checkbox" className="rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50" disabled />
                    <span className="ml-2">New matches</span>
                  </label>
                </div>
                <div className="mt-1">
                  <label className="inline-flex items-center">
                    <input type="checkbox" className="rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50" disabled />
                    <span className="ml-2">Messages</span>
                  </label>
                </div>
                <div className="mt-1">
                  <label className="inline-flex items-center">
                    <input type="checkbox" className="rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50" disabled />
                    <span className="ml-2">Activity suggestions</span>
                  </label>
                </div>
              </div>
              
              <div className="pt-4 border-t border-gray-200">
                <h3 className="font-medium">Privacy Settings</h3>
                <div className="mt-2">
                  <label className="inline-flex items-center">
                    <input type="checkbox" className="rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50" disabled />
                    <span className="ml-2">Show my profile to other users</span>
                  </label>
                </div>
                <div className="mt-1">
                  <label className="inline-flex items-center">
                    <input type="checkbox" className="rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50" disabled />
                    <span className="ml-2">Show my location</span>
                  </label>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Profile;