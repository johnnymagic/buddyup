import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useProfile } from '../contexts/ProfileContext';
import axios from 'axios';

const AddSportPage = () => {
  const { addUserSport, isLoading, error } = useProfile();
  const navigate = useNavigate();

  const [sports, setSports] = useState([]);
  const [loadingSports, setLoadingSports] = useState(true);
  const [sportsError, setSportsError] = useState(null);
  
  const [formData, setFormData] = useState({
    sportId: '',
    skillLevel: 'Beginner',
    yearsExperience: 0,
    notes: '',
    isPublic: true
  });

  // Fetch available sports from API
  useEffect(() => {
    const fetchSports = async () => {
      try {
        setLoadingSports(true);
        const response = await axios.get(`${import.meta.env.VITE_API_URL}/api/sports`);
        setSports(response.data);
      } catch (err) {
        setSportsError('Failed to load available sports. Please try again later.');
        console.error('Error fetching sports:', err);
      } finally {
        setLoadingSports(false);
      }
    };

    fetchSports();
  }, []);

  // Handle form input changes
  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    
    if (type === 'checkbox') {
      setFormData({ ...formData, [name]: checked });
    } else if (type === 'number') {
      setFormData({ ...formData, [name]: parseFloat(value) });
    } else {
      setFormData({ ...formData, [name]: value });
    }
  };

  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!formData.sportId) {
      alert('Please select a sport.');
      return;
    }
    
    const result = await addUserSport(formData);
    if (result) {
      navigate('/profile');
    }
  };

  if (loadingSports) {
    return <div className="flex justify-center p-8"><div className="loader">Loading sports...</div></div>;
  }

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-2xl font-bold mb-6">Add a Sport</h1>
      
      {(error || sportsError) && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          {error || sportsError}
        </div>
      )}
      
      <form onSubmit={handleSubmit} className="bg-white shadow-md rounded p-6">
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="sportId">
            Sport
          </label>
          <select
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            id="sportId"
            name="sportId"
            value={formData.sportId}
            onChange={handleChange}
            required
          >
            <option value="">Select a sport</option>
            {sports.map(sport => (
              <option key={sport.sportId} value={sport.sportId}>
                {sport.name}
              </option>
            ))}
          </select>
        </div>
        
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="skillLevel">
            Skill Level
          </label>
          <select
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            id="skillLevel"
            name="skillLevel"
            value={formData.skillLevel}
            onChange={handleChange}
            required
          >
            <option value="Beginner">Beginner</option>
            <option value="Intermediate">Intermediate</option>
            <option value="Advanced">Advanced</option>
            <option value="Expert">Expert</option>
          </select>
        </div>
        
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="yearsExperience">
            Years of Experience
          </label>
          <input
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            id="yearsExperience"
            type="number"
            name="yearsExperience"
            value={formData.yearsExperience}
            onChange={handleChange}
            min="0"
            max="50"
          />
        </div>
        
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="notes">
            Notes (Optional)
          </label>
          <textarea
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            id="notes"
            name="notes"
            value={formData.notes}
            onChange={handleChange}
            rows="3"
            placeholder="Share any additional details about your experience with this sport"
          ></textarea>
        </div>
        
        <div className="mb-6">
          <label className="flex items-center">
            <input
              type="checkbox"
              name="isPublic"
              checked={formData.isPublic}
              onChange={handleChange}
              className="mr-2"
            />
            <span className="text-gray-700">Make this sport visible to other users</span>
          </label>
        </div>
        
        <div className="flex items-center justify-between">
          <button
            className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline"
            type="submit"
            disabled={isLoading}
          >
            {isLoading ? 'Adding...' : 'Add Sport'}
          </button>
          <button
            className="bg-gray-300 hover:bg-gray-400 text-gray-800 font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline"
            type="button"
            onClick={() => navigate('/profile')}
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
};

export default AddSportPage;