import React from 'react';
import BuddyList from '../components/matching/BuddyList';
import { useMatching } from '../hooks/useMatching';

/**
 * Find Buddies page component for matching with workout partners
 */
const FindBuddies = () => {
  const { pendingRequests, receivedRequests, currentMatches, loading } = useMatching();
  
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-6">Find Workout Buddies</h1>
      
      {/* Display match stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <div className="bg-white rounded-lg shadow p-4">
          <h3 className="font-semibold text-lg text-gray-700">Current Matches</h3>
          <p className="text-3xl font-bold text-blue-600 mt-2">
            {loading ? '...' : currentMatches.length}
          </p>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <h3 className="font-semibold text-lg text-gray-700">Sent Requests</h3>
          <p className="text-3xl font-bold text-blue-600 mt-2">
            {loading ? '...' : pendingRequests.length}
          </p>
        </div>
        <div className="bg-white rounded-lg shadow p-4">
          <h3 className="font-semibold text-lg text-gray-700">Received Requests</h3>
          <p className="text-3xl font-bold text-blue-600 mt-2">
            {loading ? '...' : receivedRequests.length}
          </p>
        </div>
      </div>
      
      {/* Tabs for different match states */}
      {receivedRequests.length > 0 && (
        <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-8">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg className="h-5 w-5 text-yellow-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm text-yellow-700">
                You have <span className="font-bold">{receivedRequests.length}</span> pending buddy {receivedRequests.length === 1 ? 'request' : 'requests'}.
                <a href="/messages" className="font-medium underline text-yellow-700 hover:text-yellow-600 ml-1">
                  View requests
                </a>
              </p>
            </div>
          </div>
        </div>
      )}
      
      {/* Main buddy list component */}
      <BuddyList />
    </div>
  );
};

export default FindBuddies;