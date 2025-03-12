import React, { useState } from 'react';
import { useMatching } from '../hooks/useMatching';
import { useMessaging } from '../hooks/useMessaging';
import ConversationList from '../components/messaging/ConversationList';
import ChatBox from '../components/messaging/ChatBox';
import MatchConfirmation from '../components/matching/MatchConfirmation';

/**
 * Messages page component for handling conversations and match requests
 */
const Messages = () => {
  const { receivedRequests, respondToRequest } = useMatching();
  const { conversations, activeConversation, selectConversation, loading } = useMessaging();
  
  const [activeTab, setActiveTab] = useState('conversations');

  const handleTabChange = (tab) => {
    setActiveTab(tab);
  };

  const handleAcceptRequest = async (matchId) => {
    try {
      await respondToRequest(matchId, true);
    } catch (error) {
      console.error('Error accepting match request', error);
    }
  };

  const handleDeclineRequest = async (matchId) => {
    try {
      await respondToRequest(matchId, false);
    } catch (error) {
      console.error('Error declining match request', error);
    }
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-6">Messages</h1>
      
      {/* Tabs navigation */}
      <div className="mb-6 border-b border-gray-200">
        <nav className="flex -mb-px space-x-8">
          <button
            className={`pb-4 px-1 ${
              activeTab === 'conversations'
                ? 'border-b-2 border-blue-500 text-blue-600 font-medium'
                : 'border-b-2 border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
            onClick={() => handleTabChange('conversations')}
          >
            Conversations
            {conversations.length > 0 && (
              <span className="ml-2 px-2 py-0.5 text-xs rounded-full bg-blue-100 text-blue-800">
                {conversations.length}
              </span>
            )}
          </button>
          <button
            className={`pb-4 px-1 ${
              activeTab === 'requests'
                ? 'border-b-2 border-blue-500 text-blue-600 font-medium'
                : 'border-b-2 border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
            onClick={() => handleTabChange('requests')}
          >
            Match Requests
            {receivedRequests.length > 0 && (
              <span className="ml-2 px-2 py-0.5 text-xs rounded-full bg-red-100 text-red-800">
                {receivedRequests.length}
              </span>
            )}
          </button>
        </nav>
      </div>
      
      {/* Tab content */}
      <div className="mt-6">
        {activeTab === 'conversations' && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="md:col-span-1 bg-white rounded-lg shadow">
              <ConversationList 
                conversations={conversations}
                activeConversation={activeConversation}
                onSelectConversation={selectConversation}
                isLoading={loading}
              />
            </div>
            
            <div className="md:col-span-2 bg-white rounded-lg shadow">
              <ChatBox />
            </div>
          </div>
        )}
        
        {activeTab === 'requests' && (
          <div>
            {receivedRequests.length === 0 ? (
              <div className="text-center py-12 bg-white rounded-lg shadow">
                <svg
                  className="mx-auto h-12 w-12 text-gray-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth="2"
                    d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
                  ></path>
                </svg>
                <h3 className="mt-2 text-lg font-medium text-gray-900">No match requests</h3>
                <p className="mt-1 text-gray-500">
                  You don't have any pending match requests right now.
                </p>
                <div className="mt-6">
                  <button
                    type="button"
                    className="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                    onClick={() => handleTabChange('conversations')}
                  >
                    View Conversations
                  </button>
                </div>
              </div>
            ) : (
              <div className="space-y-6">
                {receivedRequests.map((request) => (
                  <MatchConfirmation
                    key={request.matchId}
                    match={request}
                    onAccept={() => handleAcceptRequest(request.matchId)}
                    onDecline={() => handleDeclineRequest(request.matchId)}
                  />
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default Messages;