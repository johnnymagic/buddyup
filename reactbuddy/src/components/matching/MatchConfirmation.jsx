import React, { useState } from 'react';
import Button from '../common/Button';

/**
 * MatchConfirmation component for responding to buddy match requests
 * 
 * @param {Object} props - Component props
 * @param {Object} props.match - Match request data
 * @param {Function} props.onAccept - Function to call when accepting the match
 * @param {Function} props.onDecline - Function to call when declining the match
 */
const MatchConfirmation = ({ match, onAccept, onDecline }) => {
  const [loading, setLoading] = useState(false);
  const [expanded, setExpanded] = useState(false);
  
  const handleAccept = async () => {
    setLoading(true);
    try {
      await onAccept();
    } catch (error) {
      console.error('Error accepting match:', error);
    } finally {
      setLoading(false);
    }
  };
  
  const handleDecline = async () => {
    setLoading(true);
    try {
      await onDecline();
    } catch (error) {
      console.error('Error declining match:', error);
    } finally {
      setLoading(false);
    }
  };
  
  const formatDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString([], { year: 'numeric', month: 'short', day: 'numeric' });
  };
  
  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden">
      <div className="p-6">
        <div className="flex items-start">
          <div className="flex-shrink-0">
            {match.requester?.profilePictureUrl ? (
              <img
                src={match.requester.profilePictureUrl}
                alt={match.requester.firstName}
                className="h-12 w-12 rounded-full object-cover"
              />
            ) : (
              <div className="h-12 w-12 rounded-full bg-blue-100 flex items-center justify-center">
                <span className="text-blue-800 font-bold">
                  {match.requester?.firstName?.charAt(0) || '?'}
                </span>
              </div>
            )}
          </div>
          
          <div className="ml-4 flex-1">
            <div className="flex justify-between">
              <h3 className="text-lg font-medium text-gray-900">
                {match.requester?.firstName || 'Someone'} wants to be your workout buddy
              </h3>
              <span className="text-xs text-gray-500">
                {formatDate(match.requestedAt)}
              </span>
            </div>
            
            <div className="mt-2 bg-blue-50 rounded-md py-2 px-3 inline-flex items-center">
              <img 
                src={match.sport?.iconUrl} 
                alt={match.sport?.name} 
                className="h-5 w-5 mr-2"
                onError={(e) => {
                  e.target.onerror = null;
                  e.target.style.display = 'none';
                }}
              />
              <span className="font-medium text-blue-800">{match.sport?.name}</span>
              {match.requesterSkillLevel && (
                <span className="ml-2 text-xs bg-blue-100 text-blue-800 px-2 rounded-full">
                  {match.requesterSkillLevel}
                </span>
              )}
            </div>
            
            {match.message && (
              <div className="mt-2 text-gray-700">
                <p>"{match.message}"</p>
              </div>
            )}
            
            <button
              type="button"
              className="mt-2 text-sm text-blue-600 hover:text-blue-800 focus:outline-none"
              onClick={() => setExpanded(!expanded)}
            >
              {expanded ? 'Less details' : 'More details'}
            </button>
            
            {expanded && (
              <div className="mt-3 border-t border-gray-200 pt-3">
                <dl className="grid grid-cols-1 gap-x-4 gap-y-2 sm:grid-cols-2">
                  {match.requester?.preferredDays?.length > 0 && (
                    <div className="sm:col-span-1">
                      <dt className="text-sm font-medium text-gray-500">Available on</dt>
                      <dd className="mt-1 text-sm text-gray-900">
                        {match.requester.preferredDays.join(', ')}
                      </dd>
                    </div>
                  )}
                  
                  {match.requester?.preferredTimes?.length > 0 && (
                    <div className="sm:col-span-1">
                      <dt className="text-sm font-medium text-gray-500">Preferred times</dt>
                      <dd className="mt-1 text-sm text-gray-900">
                        {match.requester.preferredTimes.join(', ')}
                      </dd>
                    </div>
                  )}
                  
                  {match.requester?.maxTravelDistance && (
                    <div className="sm:col-span-1">
                      <dt className="text-sm font-medium text-gray-500">Travel distance</dt>
                      <dd className="mt-1 text-sm text-gray-900">
                        Up to {match.requester.maxTravelDistance} km
                      </dd>
                    </div>
                  )}
                  
                  {match.distance && (
                    <div className="sm:col-span-1">
                      <dt className="text-sm font-medium text-gray-500">Distance from you</dt>
                      <dd className="mt-1 text-sm text-gray-900">
                        {match.distance.toFixed(1)} km
                      </dd>
                    </div>
                  )}
                  
                  {match.requester?.bio && (
                    <div className="sm:col-span-2">
                      <dt className="text-sm font-medium text-gray-500">Bio</dt>
                      <dd className="mt-1 text-sm text-gray-900">
                        {match.requester.bio}
                      </dd>
                    </div>
                  )}
                </dl>
              </div>
            )}
          </div>
        </div>
        
        <div className="mt-5 sm:mt-6 sm:flex sm:flex-row-reverse">
          <Button
            onClick={handleAccept}
            disabled={loading}
            className="w-full sm:ml-3 sm:w-auto bg-blue-600 hover:bg-blue-700 text-white"
          >
            Accept
          </Button>
          <Button
            onClick={handleDecline}
            disabled={loading}
            className="mt-3 sm:mt-0 w-full sm:w-auto bg-white hover:bg-gray-50 text-gray-700 border border-gray-300"
          >
            Decline
          </Button>
        </div>
      </div>
    </div>
  );
};

export default MatchConfirmation;