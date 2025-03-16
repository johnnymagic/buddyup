import React, { useState } from 'react';
import { api } from '../../services/ApiService'; // Adjust path as needed

const MinimalUserDisplay = () => {
  const [userData, setUserData] = useState(null);
  const [loading, setLoading] = useState(false);
  
  const loadData = async () => {
    setLoading(true);
    try {
      // Direct API call with minimal parameters
      const response = await api.get('/api/admin/users?Page=1&PageSize=10&Search=.&Sport=all');
      console.log('[MINIMAL] API response:', response);
      setUserData(response);
    } catch (error) {
      console.error('[MINIMAL] Error:', error);
    } finally {
      setLoading(false);
    }
  };
  
  // Super simple styling with bright colors to ensure visibility
  return (
    <div style={{
      margin: '20px',
      padding: '20px',
      border: '5px solid red',
      background: 'yellow'
    }}>
      <h2 style={{ color: 'black', fontWeight: 'bold', fontSize: '24px' }}>
        Minimal User Display
      </h2>
      
      {!userData ? (
        <button 
          onClick={loadData}
          style={{
            padding: '10px 20px',
            background: 'blue',
            color: 'white',
            fontWeight: 'bold',
            border: 'none',
            borderRadius: '5px',
            cursor: 'pointer'
          }}
        >
          {loading ? 'Loading...' : 'Load User Data'}
        </button>
      ) : (
        <div>
          <p style={{ fontWeight: 'bold', color: 'blue' }}>
            Data loaded - Found {userData.items?.length || 0} users
          </p>
          
          {userData.items && userData.items.length > 0 ? (
            <div style={{ 
              marginTop: '20px', 
              padding: '10px',
              border: '3px solid green',
              background: 'white'
            }}>
              <h3 style={{ fontWeight: 'bold' }}>First User:</h3>
              <p>Name: {userData.items[0].firstName} {userData.items[0].lastName}</p>
              <p>Email: {userData.items[0].email}</p>
              <p>Status: {userData.items[0].active ? 'Active' : 'Inactive'}</p>
            </div>
          ) : (
            <p style={{ color: 'red', fontWeight: 'bold' }}>No users found</p>
          )}
          
          <div style={{ 
            marginTop: '20px',
            background: 'lightgray',
            padding: '10px',
            maxHeight: '200px',
            overflow: 'auto',
            border: '1px solid black'
          }}>
            <pre>{JSON.stringify(userData, null, 2)}</pre>
          </div>
        </div>
      )}
    </div>
  );
};

export default MinimalUserDisplay;