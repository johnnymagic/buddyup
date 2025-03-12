import React, { useState } from 'react';
import { useMatching } from '../../hooks/useMatching';
import BuddyCard from './BuddyCard';
import FilterOptions from './FilterOptions';
import Spinner from '../common/Spinner';

const BuddyList = () => {
  const { 
    potentialMatches, 
    loading, 
    error, 
    filters, 
    updateFilters, 
    sendMatchRequest 
  } = useMatching();
  
  const [expandedFilter, setExpandedFilter] = useState(false);

  const handleSendRequest = async (buddyId, sportId) => {
    try {
      await sendMatchRequest(buddyId, sportId);
    } catch (error) {
      console.error('Error sending match request:', error);
    }
  };

  const toggleFilter = () => {
    setExpandedFilter(!expandedFilter);
  };

  return (
    <div className="max-w-6xl mx-auto px-4 py-6">
      <div className="mb-8">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-2xl font-bold">Find Workout Buddies</h2>
          <button 
            onClick={toggleFilter}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            {expandedFilter ? 'Hide Filters' : 'Show Filters'}
          </button>
        </div>
        
        {expandedFilter && (
          <FilterOptions 
            filters={filters} 
            updateFilters={updateFilters} 
          />
        )}
      </div>
      
      {loading ? (
        <div className="flex justify-center items-center py-12">
          <Spinner size="lg" />
        </div>
      ) : error ? (
        <div className="text-center text-red-500 py-8">
          <p>Error loading potential buddies.</p>
          <p>{error}</p>
        </div>
      ) : potentialMatches.length === 0 ? (
        <div className="text-center py-12">
          <h3 className="text-xl font-semibold mb-2">No matches found</h3>
          <p className="text-gray-600 mb-6">
            Try adjusting your filters or adding more sports to your profile to find more workout buddies.
          </p>
          <button 
            onClick={toggleFilter}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Adjust Filters
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {potentialMatches.map(buddy => (
            <BuddyCard 
              key={`${buddy.userId}-${buddy.sportId}`}
              buddy={buddy}
              onSendRequest={handleSendRequest}
            />
          ))}
        </div>
      )}
    </div>
  );
};

export default BuddyList;