import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

/**
 * Home page component - landing page for the application
 */
const Home = () => {
  const { isAuthenticated } = useAuth();

  return (
    
    <div className="bg-gradient-to-b from-blue-50 to-white">




      {/* Hero Section */}
      <section className="py-16 md:py-24">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="md:flex md:items-center md:justify-between">
            <div className="md:w-1/2 md:pr-8">
              <h1 className="text-4xl md:text-5xl font-bold text-gray-900 leading-tight">
                Find Your Perfect Workout Buddy
              </h1>
              <p className="mt-4 text-xl text-gray-600 max-w-3xl">
                Connect with people who share your fitness interests and goals. 
                Make workouts more fun and stay motivated together.
              </p>
              <div className="mt-8 flex flex-wrap gap-4">
                {isAuthenticated ? (
                  <Link
                    to="/find-buddies"
                    className="px-6 py-3 bg-blue-600 text-white font-medium rounded-md shadow-md hover:bg-blue-700 transition duration-300"
                  >
                    Find Buddies
                  </Link>
                ) : (
                  <Link
                    to="/login"
                    className="px-6 py-3 bg-blue-600 text-white font-medium rounded-md shadow-md hover:bg-blue-700 transition duration-300"
                  >
                    Join Now
                  </Link>
                )}
                <Link
                  to="/activities"
                  className="px-6 py-3 bg-white text-blue-600 font-medium rounded-md shadow-md border border-blue-200 hover:bg-blue-50 transition duration-300"
                >
                  Explore Activities
                </Link>
              </div>
            </div>
            <div className="mt-8 md:mt-0 md:w-1/2">
              <img
                src="/images/hero-image.jpg"
                alt="People exercising together"
                className="rounded-lg shadow-xl"
                onError={(e) => {
                  e.target.onerror = null;
                  e.target.src = 'https://placehold.co/600x400?text=Workout+Together';
                }}
              />
            </div>
          </div>
        </div>
      </section>

      {/* How It Works */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-3xl font-bold text-gray-900">How Buddy Up Works</h2>
            <p className="mt-4 text-xl text-gray-600 max-w-3xl mx-auto">
              Finding a workout partner has never been easier
            </p>
          </div>

          <div className="mt-12 grid grid-cols-1 gap-8 md:grid-cols-3">
            <div className="bg-blue-50 rounded-lg p-6 shadow-md">
              <div className="w-12 h-12 bg-blue-600 text-white rounded-full flex items-center justify-center text-xl font-bold mb-4">
                1
              </div>
              <h3 className="text-xl font-semibold text-gray-900">Create Your Profile</h3>
              <p className="mt-2 text-gray-600">
                Sign up and create your profile with your favorite sports, skill level, and schedule.
              </p>
            </div>

            <div className="bg-blue-50 rounded-lg p-6 shadow-md">
              <div className="w-12 h-12 bg-blue-600 text-white rounded-full flex items-center justify-center text-xl font-bold mb-4">
                2
              </div>
              <h3 className="text-xl font-semibold text-gray-900">Match with Buddies</h3>
              <p className="mt-2 text-gray-600">
                Browse potential workout partners filtered by sport, location, and availability.
              </p>
            </div>

            <div className="bg-blue-50 rounded-lg p-6 shadow-md">
              <div className="w-12 h-12 bg-blue-600 text-white rounded-full flex items-center justify-center text-xl font-bold mb-4">
                3
              </div>
              <h3 className="text-xl font-semibold text-gray-900">Meet & Exercise</h3>
              <p className="mt-2 text-gray-600">
                Connect through the app, schedule your workout, and enjoy exercising together!
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Sports Section */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-3xl font-bold text-gray-900">Popular Sports</h2>
            <p className="mt-4 text-xl text-gray-600 max-w-3xl mx-auto">
              Find partners for a wide variety of activities
            </p>
          </div>

          <div className="mt-12 grid grid-cols-2 gap-6 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5">
            {['Tennis', 'Running', 'Cycling', 'Swimming', 'Yoga', 'Basketball', 'Soccer', 'Hiking', 'Golf', 'Volleyball'].map((sport) => (
              <div key={sport} className="bg-white rounded-lg p-4 shadow-md text-center hover:shadow-lg transition-shadow">
                <div className="h-12 flex items-center justify-center mb-2">
                  <img 
                    src={`/images/sports/${sport.toLowerCase()}.svg`} 
                    alt={sport} 
                    className="h-10"
                    onError={(e) => {
                      e.target.onerror = null;
                      e.target.style.display = 'none';
                    }}
                  />
                </div>
                <h3 className="font-medium text-gray-900">{sport}</h3>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Testimonials */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-3xl font-bold text-gray-900">What Our Users Say</h2>
          </div>

          <div className="mt-12 grid grid-cols-1 gap-8 md:grid-cols-3">
            {[
              {
                name: 'Sarah J.',
                sport: 'Running',
                quote: "Found my perfect running partner within a week. We've been training for a half marathon together!"
              },
              {
                name: 'Mike T.',
                sport: 'Tennis',
                quote: 'As a beginner, I was nervous about finding someone patient enough to play with me. Buddy Up matched me with an amazing mentor.'
              },
              {
                name: 'Priya K.',
                sport: 'Yoga',
                quote: "Meeting up with my yoga buddy keeps me accountable. We've tried so many new classes together!"
              }
            ].map((testimonial, index) => (
              <div key={index} className="bg-blue-50 rounded-lg p-6 shadow-md">
                <div className="flex items-center mb-4">
                  <div className="w-10 h-10 bg-blue-600 text-white rounded-full flex items-center justify-center font-medium">
                    {testimonial.name.charAt(0)}
                  </div>
                  <div className="ml-3">
                    <h3 className="text-lg font-semibold text-gray-900">{testimonial.name}</h3>
                    <p className="text-sm text-gray-600">{testimonial.sport} Enthusiast</p>
                  </div>
                </div>
                <p className="text-gray-700 italic">"{testimonial.quote}"</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-16 bg-blue-600">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl font-bold text-white">Ready to Find Your Workout Buddy?</h2>
          <p className="mt-4 text-xl text-blue-100 max-w-3xl mx-auto">
            Join thousands of fitness enthusiasts who've found their perfect match.
          </p>
          <div className="mt-8">
            {isAuthenticated ? (
              <Link
                to="/find-buddies"
                className="px-8 py-3 bg-white text-blue-600 font-medium rounded-md shadow-md hover:bg-blue-50 transition duration-300"
              >
                Find Buddies Now
              </Link>
            ) : (
              <Link
                to="/login"
                className="px-8 py-3 bg-white text-blue-600 font-medium rounded-md shadow-md hover:bg-blue-50 transition duration-300"
              >
                Sign Up Free
              </Link>
            )}
          </div>
        </div>
      </section>
    </div>
  );
};

export default Home;