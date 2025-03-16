import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Auth0Provider } from '@auth0/auth0-react';

// Context providers
import { AuthProvider } from './context/AuthContext';
import { ProfileProvider } from './context/ProfileContext';
import { MatchingProvider } from './context/MatchingContext';
import { MessagingProvider } from './context/MessagingContext';
import { AdminProvider } from './context/AdminContext'; // Make sure this import matches your file structure
import { ActivityProvider } from './context/ActivityContext';

// Components
import Navbar from './components/common/Navbar';
import Footer from './components/common/Footer';
import ProtectedRoute from './components/auth/ProtectedRoute';
import './index.css';

// Pages
import Home from './pages/Home';
import Login from './pages/Login';
import Profile from './pages/Profile';
import FindBuddies from './pages/FindBuddies';
import Messages from './pages/Messages';
import ActivitySuggestions from './pages/ActivitySuggestions';
import ActivityDetail from './pages/ActivityDetail';
import ActivityForm from './pages/ActivityForm';
import Verification from './pages/Verification';
import Admin from './pages/Admin';

function App() {
  return (
    <Auth0Provider
      domain={import.meta.env.VITE_AUTH0_DOMAIN}
      clientId={import.meta.env.VITE_AUTH0_CLIENT_ID}
      authorizationParams={{
        redirect_uri: window.location.origin,
        audience: import.meta.env.VITE_AUTH0_AUDIENCE,  // "https://buddyup-api"
        scope: 'openid profile email'
      }}
      useRefreshTokens={true}
      cacheLocation="localstorage"
    >
      <AuthProvider>
        <ProfileProvider>
          <MatchingProvider>
            <MessagingProvider>
              <AdminProvider>
                <ActivityProvider>
                  <Router>
                    <div className="flex flex-col min-h-screen">
                      <Navbar />
                      <main className="flex-grow">
                        <Routes>
                          {/* Public routes */}
                          <Route path="/" element={<Home />} />
                          <Route path="/login" element={<Login />} />

                          {/* Protected routes */}
                          <Route
                            path="/profile"
                            element={
                              <ProtectedRoute>
                                <Profile />
                              </ProtectedRoute>
                            }
                          />
                          <Route
                            path="/find-buddies"
                            element={
                              <ProtectedRoute>
                                <FindBuddies />
                              </ProtectedRoute>
                            }
                          />
                          <Route
                            path="/messages"
                            element={
                              <ProtectedRoute>
                                <Messages />
                              </ProtectedRoute>
                            }
                          />

                          {/* Activity routes */}
                          <Route
                            path="/activities"
                            element={
                              <ProtectedRoute>
                                <ActivitySuggestions />
                              </ProtectedRoute>
                            }
                          />
                          <Route
                            path="/activity/:activityId"
                            element={
                              <ProtectedRoute>
                                <ActivityDetail />
                              </ProtectedRoute>
                            }
                          />
                          <Route
                            path="/activities/create"
                            element={
                              <ProtectedRoute>
                                <ActivityForm />
                              </ProtectedRoute>
                            }
                          />
                          <Route
                            path="/activities/edit/:activityId"
                            element={
                              <ProtectedRoute>
                                <ActivityForm />
                              </ProtectedRoute>
                            }
                          />

                          <Route
                            path="/verification"
                            element={
                              <ProtectedRoute>
                                <Verification />
                              </ProtectedRoute>
                            }
                          />

                          {/* Admin routes */}
                          <Route
                            path="/admin/*"
                            element={
                              <ProtectedRoute adminOnly>
                                <Admin />
                              </ProtectedRoute>
                            }
                          />

                          {/* Fallback route */}
                          <Route path="*" element={<Navigate to="/" replace />} />
                        </Routes>
                      </main>
                      <Footer />
                    </div>
                  </Router>
                </ActivityProvider>
              </AdminProvider>
            </MessagingProvider>
          </MatchingProvider>
        </ProfileProvider>
      </AuthProvider>
    </Auth0Provider>
  );
}

export default App;