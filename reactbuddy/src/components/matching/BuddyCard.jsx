import React from 'react';
import Button from '../common/Button';

const BuddyCard = ({ buddy, onSendRequest }) => {
  const handleSendRequest = () => {
    onSendRequest(buddy.userId, buddy.sportId);
  };
  
  // Format days array into readable string
  const formatDays = (days) => {
    if (!days || days.length === 0) return 'Flexible';
    
    // If all days are selected
    if (days.length === 7) return 'Any day';
    
    // Short format for each day
    const dayMap = {
      'monday': 'Mon',
      'tuesday': 'Tue',
      'wednesday': 'Wed',
      'thursday': 'Thu',
      'friday': 'Fri',
      'saturday': 'Sat',
      'sunday': 'Sun'
    };
    
    return days.map(day => dayMap[day] || day).join(', ');
  };
  
  // Format times array into readable string
  const formatTimes = (times) => {
    if (!times || times.length === 0) return 'Flexible';
    
    // If all times are selected
    if (times.length === 4) return 'Any time';
    
    return times.map(time => time.charAt(0).toUpperCase() + time.slice(1)).join(', ');
  };
  
  // Calculate distance string
  const formatDistance = (distance) => {
    if (!distance) return '';
    
    if (distance < 1) {
      return 'Less than 1 km away';
    } else {
      return `${Math.round(distance)} km away`;
    }
  };
  
  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden">
      <div className="relative">
        {buddy.profilePictureUrl ? (
          <img 
            src={buddy.profilePictureUrl} 
            alt={`${buddy.firstName}'s profile`}
            className="w-full h-48 object-cover"
          />
        ) : (
          <div className="w-full h-48 bg-gray-200 flex items-center justify-center">
            <span className="text-4xl text-gray-400">
              {buddy.firstName.charAt(0)}
            </span>
          </div>
        )}
        
        {buddy.verified && (
          <div className="absolute top-2 right-2 bg-blue-500 text-white text-xs font-bold px-2 py-1 rounded-full">
            Verified
          </div>
        )}
      </div>
      
      <div className="p-4">
        <div className="flex justify-between items-start">
          <h3 className="text-xl font-semibold">{buddy.firstName}</h3>
          <div className="text-xs bg-gray-100 text-gray-800 px-2 py-1 rounded-full">
            {formatDistance(buddy.distance)}
          </div>
        </div>
        
        <div className="mt-3 py-2 px-3 bg-blue-50 rounded-md">
          <div className="flex items-center">
            <span className="font-semibold">{buddy.sport}</span>
            <span className="mx-2">•</span>
            <span className="text-sm capitalize">{buddy.skillLevel}</span>
          </div>
        </div>
        
        {buddy.bio && (
          <p className="mt-3 text-gray-600 text-sm line-clamp-3">{buddy.bio}</p>
        )}
        
        <div className="mt-4 grid grid-cols-2 gap-2 text-xs text-gray-500">
          <div>
            <span className="block font-medium">Available:</span>
            <span>{formatDays(buddy.preferredDays)}</span>
          </div>
          <div>
            <span className="block font-medium">Times:</span>
            <span>{formatTimes(buddy.preferredTimes)}</span>
          </div>
        </div>
        
        <div className="mt-4 pt-4 border-t border-gray-100">
          <Button
            onClick={handleSendRequest}
            className="w-full bg-blue-600 hover:bg-blue-700 text-white"
          >
            Send Buddy Request
          </Button>
        </div>
      </div>
    </div>
  );
};

export default BuddyCard;