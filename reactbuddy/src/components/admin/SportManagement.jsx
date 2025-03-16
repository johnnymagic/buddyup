import React from 'react';
import { SportsProvider } from '../../context/SportsContext';
import SportsList from '../sports/SportsList';
import SportForm from '../sports/SportForm';

const SportManagement = () => {
  return (
    <SportsProvider>
      <div className="container mx-auto px-4 py-6">
        <div className="mb-6">
          <h1 className="text-2xl font-bold">Sports Management</h1>
          <p className="text-gray-600">
            Manage sports available in the BuddyUp platform. Users can select these sports as their interests.
          </p>
        </div>
        
        <div className="flex flex-col lg:flex-row gap-6">
          <div className="w-full lg:w-1/3 mb-6 lg:mb-0">
            <SportForm />
          </div>
          <div className="w-full lg:w-2/3">
            <SportsList />
          </div>
        </div>
      </div>
    </SportsProvider>
  );
};

export default SportManagement;