import React, { useEffect } from 'react';
import { useSports } from '../../context/SportsContext';
import { FaEdit, FaToggleOn, FaToggleOff } from 'react-icons/fa';

const SportsList = () => {
  const { sports, loading, error, fetchSports, selectSport } = useSports();
  
  useEffect(() => {
    fetchSports();
  }, [fetchSports]);
  
  if (loading) {
    return (
      <div className="flex justify-center items-center py-12">
        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-500"></div>
        <span className="sr-only">Loading...</span>
      </div>
    );
  }
  
  if (error) {
    return (
      <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
        Error loading sports: {error}
      </div>
    );
  }
  
  return (
    <div className="sports-list">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold">Sports</h2>
        <button 
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
          onClick={() => selectSport(null)}
        >
          Add New Sport
        </button>
      </div>
      
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border border-gray-200">
          <thead className="bg-gray-100">
            <tr>
              <th className="px-4 py-2 border-b text-left">Name</th>
              <th className="px-4 py-2 border-b text-left">Description</th>
              <th className="px-4 py-2 border-b text-left">Icon</th>
              <th className="px-4 py-2 border-b text-left">Status</th>
              <th className="px-4 py-2 border-b text-left">Created By</th>
              <th className="px-4 py-2 border-b text-left">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {sports.length === 0 ? (
              <tr>
                <td colSpan="6" className="px-4 py-8 text-center text-gray-500">No sports found</td>
              </tr>
            ) : (
              sports.map(sport => (
                <tr key={sport.sportId} className="hover:bg-gray-50">
                  <td className="px-4 py-3">{sport.name}</td>
                  <td className="px-4 py-3">{sport.description || 'N/A'}</td>
                  <td className="px-4 py-3">
                    {sport.iconUrl ? (
                      <img
                        src={sport.iconUrl}
                        alt={sport.name}
                        className="w-8 h-8 object-contain"
                      />
                    ) : (
                      <span className="text-gray-500">No icon</span>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                      sport.isActive 
                        ? 'bg-green-100 text-green-800' 
                        : 'bg-red-100 text-red-800'
                    }`}>
                      {sport.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td className="px-4 py-3">{sport.createdBy || 'System'}</td>
                  <td className="px-4 py-3">
                    <div className="flex space-x-2">
                      <button
                        className="inline-flex items-center px-2.5 py-1.5 border border-gray-300 text-xs font-medium rounded text-blue-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                        onClick={() => selectSport(sport)}
                      >
                        <FaEdit className="mr-1" /> Edit
                      </button>
                      <button
                        className={`inline-flex items-center px-2.5 py-1.5 border text-xs font-medium rounded focus:outline-none focus:ring-2 focus:ring-offset-2 ${
                          sport.isActive
                            ? 'border-red-300 text-red-700 bg-white hover:bg-red-50 focus:ring-red-500'
                            : 'border-green-300 text-green-700 bg-white hover:bg-green-50 focus:ring-green-500'
                        }`}
                      >
                        {sport.isActive ? <FaToggleOff className="mr-1" /> : <FaToggleOn className="mr-1" />}
                        {sport.isActive ? 'Deactivate' : 'Activate'}
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default SportsList;