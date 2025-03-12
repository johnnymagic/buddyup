import React from 'react';
import Spinner from '../common/Spinner';

/**
 * ConversationList component displays a list of user conversations
 * 
 * @param {Object} props - Component props
 * @param {Array} props.conversations - List of conversation objects
 * @param {Object} props.activeConversation - Currently selected conversation
 * @param {Function} props.onSelectConversation - Function to call when a conversation is selected
 * @param {boolean} props.isLoading - Whether conversations are loading
 */
const ConversationList = ({ 
  conversations = [], 
  activeConversation, 
  onSelectConversation,
  isLoading = false
}) => {
  // Format date to a readable string
  const formatDate = (dateString) => {
    if (!dateString) return '';
    
    const date = new Date(dateString);
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);
    
    // If the date is today, show only time
    if (date >= today) {
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }
    // If the date is yesterday, show "Yesterday"
    else if (date >= yesterday) {
      return 'Yesterday';
    }
    // Otherwise show short date
    else {
      return date.toLocaleDateString([], { month: 'short', day: 'numeric' });
    }
  };

  // Truncate message text if too long
  const truncateMessage = (text, maxLength = 60) => {
    if (!text) return '';
    return text.length > maxLength
      ? text.substring(0, maxLength) + '...'
      : text;
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <Spinner size="md" />
      </div>
    );
  }

  if (conversations.length === 0) {
    return (
      <div className="p-6 text-center">
        <div className="text-gray-400 mb-2">
          <svg className="h-12 w-12 mx-auto" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
          </svg>
        </div>
        <h3 className="text-lg font-medium text-gray-900">No conversations yet</h3>
        <p className="mt-1 text-sm text-gray-500">
          Start matching with workout buddies to begin chatting.
        </p>
      </div>
    );
  }

  return (
    <div className="h-full flex flex-col">
      <div className="p-4 border-b">
        <h2 className="text-lg font-semibold">Conversations</h2>
      </div>
      
      <div className="flex-1 overflow-y-auto">
        {conversations.map((conversation) => (
          <button
            key={conversation.conversationId}
            className={`w-full text-left p-4 border-b hover:bg-gray-50 focus:outline-none ${
              activeConversation?.conversationId === conversation.conversationId
                ? 'bg-blue-50 border-l-4 border-blue-500'
                : ''
            }`}
            onClick={() => onSelectConversation(conversation)}
          >
            <div className="flex items-start">
              <div className="flex-shrink-0 relative">
                {conversation.otherUser?.profilePictureUrl ? (
                  <img
                    src={conversation.otherUser.profilePictureUrl}
                    alt={conversation.otherUser.firstName}
                    className="h-10 w-10 rounded-full"
                  />
                ) : (
                  <div className="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
                    <span className="text-blue-800 font-medium">
                      {conversation.otherUser?.firstName?.charAt(0) || '?'}
                    </span>
                  </div>
                )}
                
                {conversation.unreadCount > 0 && (
                  <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center">
                    {conversation.unreadCount}
                  </span>
                )}
              </div>
              
              <div className="ml-3 flex-1">
                <div className="flex justify-between">
                  <p className="text-sm font-medium text-gray-900">
                    {conversation.otherUser?.firstName || 'User'}
                  </p>
                  {conversation.lastMessageAt && (
                    <p className="text-xs text-gray-500">
                      {formatDate(conversation.lastMessageAt)}
                    </p>
                  )}
                </div>
                
                <div className="flex justify-between">
                  <p className="text-sm text-gray-500 truncate">
                    {truncateMessage(conversation.lastMessage || 'Start a conversation')}
                  </p>
                  
                  {conversation.sport && (
                    <span className="text-xs bg-blue-100 text-blue-800 px-2 rounded-full">
                      {conversation.sport}
                    </span>
                  )}
                </div>
              </div>
            </div>
          </button>
        ))}
      </div>
    </div>
  );
};

export default ConversationList;