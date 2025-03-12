import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { api } from '../services/ApiService';
import { useAuth } from './AuthContext';

// Create context
const MessagingContext = createContext();

export const MessagingProvider = ({ children }) => {
  const { isAuthenticated, userProfile } = useAuth();
  
  const [conversations, setConversations] = useState([]);
  const [activeConversation, setActiveConversation] = useState(null);
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [unreadCount, setUnreadCount] = useState(0);

  // Load conversations when authenticated
  useEffect(() => {
    const fetchConversations = async () => {
      if (!isAuthenticated || !userProfile) return;
      
      setLoading(true);
      setError(null);
      
      try {
        const response = await api.get('/api/messaging/conversations');
        setConversations(response);
        
        // Calculate unread messages
        const unreadMessages = response.reduce((count, conv) => 
          count + (conv.unreadCount || 0), 0);
        setUnreadCount(unreadMessages);
      } catch (err) {
        console.error('Error fetching conversations', err);
        setError('Failed to load conversations');
      } finally {
        setLoading(false);
      }
    };
    
    fetchConversations();
    
    // Set up polling for new messages (in a real app, use WebSockets instead)
    const intervalId = setInterval(fetchConversations, 30000);
    
    return () => clearInterval(intervalId);
  }, [isAuthenticated, userProfile]);

  // Load messages for active conversation
  useEffect(() => {
    const fetchMessages = async () => {
      if (!activeConversation) {
        setMessages([]);
        return;
      }
      
      setLoading(true);
      setError(null);
      
      try {
        const response = await api.get(`/api/messaging/conversations/${activeConversation.conversationId}/messages`);
        setMessages(response);
        
        // Mark messages as read
        await api.put(`/api/messaging/conversations/${activeConversation.conversationId}/read`);
        
        // Update unread count in conversations list
        setConversations(prev => 
          prev.map(conv => 
            conv.conversationId === activeConversation.conversationId
              ? { ...conv, unreadCount: 0 }
              : conv
          )
        );
        
        // Recalculate total unread count
        const unreadMessages = conversations.reduce((count, conv) => 
          conv.conversationId === activeConversation.conversationId
            ? count
            : count + (conv.unreadCount || 0), 0);
        setUnreadCount(unreadMessages);
      } catch (err) {
        console.error('Error fetching messages', err);
        setError('Failed to load messages');
      } finally {
        setLoading(false);
      }
    };
    
    fetchMessages();
    
    // Set up polling for new messages in active conversation
    const intervalId = setInterval(fetchMessages, 5000);
    
    return () => clearInterval(intervalId);
  }, [activeConversation]);

  // Set active conversation
  const selectConversation = (conversation) => {
    setActiveConversation(conversation);
  };

  // Send a message
  const sendMessage = async (content) => {
    if (!activeConversation) {
      throw new Error('No active conversation to send message to');
    }
    
    setLoading(true);
    setError(null);
    
    try {
      const newMessage = await api.post(`/api/messaging/conversations/${activeConversation.conversationId}/messages`, {
        content
      });
      
      // Add message to list
      setMessages(prev => [...prev, newMessage]);
      
      // Update last message in conversations list
      setConversations(prev => 
        prev.map(conv => 
          conv.conversationId === activeConversation.conversationId
            ? { 
                ...conv, 
                lastMessage: content,
                lastMessageAt: new Date().toISOString()
              }
            : conv
        )
      );
      
      return newMessage;
    } catch (err) {
      console.error('Error sending message', err);
      setError('Failed to send message');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Start a new conversation (usually after a match is accepted)
  const startConversation = async (userId, matchId) => {
    setLoading(true);
    setError(null);
    
    try {
      const newConversation = await api.post('/api/messaging/conversations', {
        userId,
        matchId
      });
      
      // Add to conversations list
      setConversations(prev => [newConversation, ...prev]);
      
      // Set as active
      setActiveConversation(newConversation);
      
      return newConversation;
    } catch (err) {
      console.error('Error starting conversation', err);
      setError('Failed to start conversation');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Value object to be provided to consumers
  const messagingValue = {
    conversations,
    activeConversation,
    messages,
    loading,
    error,
    unreadCount,
    selectConversation,
    sendMessage,
    startConversation
  };

  return (
    <MessagingContext.Provider value={messagingValue}>
      {children}
    </MessagingContext.Provider>
  );
};

// Custom hook for using Messaging context
export const useMessaging = () => {
  const context = useContext(MessagingContext);
  if (!context) {
    throw new Error('useMessaging must be used within a MessagingProvider');
  }
  return context;
};

export default MessagingContext;