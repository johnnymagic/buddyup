import React, { useState, useRef, useEffect } from 'react';
import { useMessaging } from '../../hooks/useMessaging';
import { useAuth } from '../../hooks/useAuth';
import Spinner from '../common/Spinner';

/**
 * ChatBox component for displaying and sending messages
 */
const ChatBox = () => {
  const { activeConversation, messages, sendMessage, loading } = useMessaging();
  const { userProfile } = useAuth();
  
  const [newMessage, setNewMessage] = useState('');
  const [sending, setSending] = useState(false);
  const messagesEndRef = useRef(null);

  // Scroll to bottom when messages change
  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleSendMessage = async (e) => {
    e.preventDefault();
    if (!newMessage.trim() || !activeConversation) return;
    
    try {
      setSending(true);
      await sendMessage(newMessage.trim());
      setNewMessage('');
    } catch (error) {
      console.error('Error sending message:', error);
    } finally {
      setSending(false);
    }
  };

  // If no conversation is selected
  if (!activeConversation) {
    return (
      <div className="h-full flex flex-col items-center justify-center p-6 bg-gray-50">
        <div className="text-center">
          <svg className="h-16 w-16 text-gray-400 mx-auto mb-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
          </svg>
          <h3 className="text-lg font-medium text-gray-900">Select a conversation</h3>
          <p className="mt-1 text-sm text-gray-500">
            Choose a conversation from the list to start chatting
          </p>
        </div>
      </div>
    );
  }

  // Format timestamp for messages
  const formatMessageTime = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  // Format date for message groups
  const formatMessageDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);
    
    // Format based on when the message was sent
    if (date.toDateString() === today.toDateString()) {
      return 'Today';
    } else if (date.toDateString() === yesterday.toDateString()) {
      return 'Yesterday';
    } else {
      return date.toLocaleDateString([], { weekday: 'long', month: 'long', day: 'numeric' });
    }
  };

  // Group messages by date
  const groupMessagesByDate = () => {
    const groups = {};
    messages.forEach(message => {
      const date = new Date(message.sentAt);
      const dateString = date.toDateString();
      if (!groups[dateString]) {
        groups[dateString] = [];
      }
      groups[dateString].push(message);
    });
    return Object.entries(groups).map(([date, messages]) => ({
      date,
      displayDate: formatMessageDate(date),
      messages
    }));
  };

  return (
    <div className="h-full flex flex-col">
      {/* Chat header */}
      <div className="p-4 border-b flex items-center">
        <div className="flex-shrink-0">
          {activeConversation.otherUser?.profilePictureUrl ? (
            <img
              src={activeConversation.otherUser.profilePictureUrl}
              alt={activeConversation.otherUser.firstName}
              className="h-10 w-10 rounded-full"
            />
          ) : (
            <div className="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
              <span className="text-blue-800 font-medium">
                {activeConversation.otherUser?.firstName?.charAt(0) || '?'}
              </span>
            </div>
          )}
        </div>
        
        <div className="ml-3">
          <h2 className="text-lg font-semibold">
            {activeConversation.otherUser?.firstName || 'User'}
          </h2>
          {activeConversation.sport && (
            <div className="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded-full inline-block">
              {activeConversation.sport}
            </div>
          )}
        </div>
      </div>
      
      {/* Messages area */}
      <div className="flex-1 overflow-y-auto p-4 bg-gray-50">
        {loading ? (
          <div className="flex justify-center items-center h-full">
            <Spinner size="md" />
          </div>
        ) : messages.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-full text-center">
            <p className="text-gray-500 mb-2">No messages yet</p>
            <p className="text-sm text-gray-400">
              Send a message to start the conversation
            </p>
          </div>
        ) : (
          <div className="space-y-6">
            {groupMessagesByDate().map((group) => (
              <div key={group.date}>
                <div className="flex justify-center mb-4">
                  <span className="px-3 py-1 bg-gray-200 rounded-full text-xs text-gray-600">
                    {group.displayDate}
                  </span>
                </div>
                
                <div className="space-y-3">
                  {group.messages.map((message) => {
                    const isOwnMessage = message.senderId === userProfile?.userId;
                    
                    return (
                      <div 
                        key={message.messageId} 
                        className={`flex ${isOwnMessage ? 'justify-end' : 'justify-start'}`}
                      >
                        <div 
                          className={`max-w-xs md:max-w-md lg:max-w-lg px-4 py-2 rounded-lg ${
                            isOwnMessage 
                              ? 'bg-blue-600 text-white rounded-br-none' 
                              : 'bg-white text-gray-800 rounded-bl-none shadow'
                          }`}
                        >
                          <div className="text-sm">{message.content}</div>
                          <div 
                            className={`text-xs mt-1 ${
                              isOwnMessage ? 'text-blue-200' : 'text-gray-500'
                            }`}
                          >
                            {formatMessageTime(message.sentAt)}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            ))}
            <div ref={messagesEndRef} />
          </div>
        )}
      </div>
      
      {/* Message input */}
      <div className="p-4 border-t">
        <form onSubmit={handleSendMessage} className="flex items-center">
          <input
            type="text"
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            placeholder="Type a message..."
            className="flex-1 px-4 py-2 border rounded-l-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            disabled={sending}
          />
          <button
            type="submit"
            className="px-4 py-2 bg-blue-600 text-white rounded-r-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
            disabled={sending || !newMessage.trim()}
          >
            {sending ? (
              <span className="flex items-center">
                <Spinner size="sm" color="white" />
                <span className="ml-2">Sending...</span>
              </span>
            ) : (
              'Send'
            )}
          </button>
        </form>
      </div>
    </div>
  );
};

export default ChatBox;