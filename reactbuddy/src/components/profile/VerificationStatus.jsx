import React from 'react';
import { useProfile } from '../../hooks/useProfile';
import Spinner from '../common/Spinner';

/**
 * Verification Status component
 * Displays the current verification status of the user
 */
const VerificationStatus = () => {
  const { verificationStatus, loading } = useProfile();

  if (loading) {
    return (
      <div className="flex justify-center py-4">
        <Spinner size="md" />
      </div>
    );
  }

  // Status-specific UI
  const renderStatusContent = () => {
    if (!verificationStatus) {
      return (
        <div className="flex items-center bg-gray-100 p-4 rounded-lg">
          <div className="flex-shrink-0 mr-4">
            <svg className="h-10 w-10 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <h3 className="font-medium text-gray-900">Not Verified</h3>
            <p className="text-sm text-gray-500 mt-1">
              You haven't started the verification process yet.
            </p>
          </div>
        </div>
      );
    }

    const status = verificationStatus.status?.toLowerCase();

    if (status === 'pending') {
      return (
        <div className="flex items-center bg-yellow-50 p-4 rounded-lg border-l-4 border-yellow-400">
          <div className="flex-shrink-0 mr-4">
            <svg className="h-10 w-10 text-yellow-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <h3 className="font-medium text-yellow-800">Verification In Progress</h3>
            <p className="text-sm text-yellow-700 mt-1">
              We're currently processing your verification. This typically takes 1-2 business days.
            </p>
            {verificationStatus.initiatedAt && (
              <p className="text-xs text-yellow-600 mt-2">
                Submitted on: {new Date(verificationStatus.initiatedAt).toLocaleDateString()}
              </p>
            )}
          </div>
        </div>
      );
    }

    if (status === 'verified') {
      return (
        <div className="flex items-center bg-green-50 p-4 rounded-lg border-l-4 border-green-400">
          <div className="flex-shrink-0 mr-4">
            <svg className="h-10 w-10 text-green-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
            </svg>
          </div>
          <div>
            <h3 className="font-medium text-green-800">Verified</h3>
            <p className="text-sm text-green-700 mt-1">
              Your identity has been verified. You'll see a verification badge on your profile.
            </p>
            {verificationStatus.completedAt && (
              <p className="text-xs text-green-600 mt-2">
                Verified on: {new Date(verificationStatus.completedAt).toLocaleDateString()}
              </p>
            )}
          </div>
        </div>
      );
    }

    if (status === 'failed') {
      return (
        <div className="flex items-center bg-red-50 p-4 rounded-lg border-l-4 border-red-400">
          <div className="flex-shrink-0 mr-4">
            <svg className="h-10 w-10 text-red-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <h3 className="font-medium text-red-800">Verification Failed</h3>
            <p className="text-sm text-red-700 mt-1">
              We couldn't verify your identity. Please try again or contact support for assistance.
            </p>
          </div>
        </div>
      );
    }

    // Default/unknown status
    return (
      <div className="flex items-center bg-gray-100 p-4 rounded-lg">
        <div className="flex-shrink-0 mr-4">
          <svg className="h-10 w-10 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <div>
          <h3 className="font-medium text-gray-900">Status: {verificationStatus.status}</h3>
          <p className="text-sm text-gray-500 mt-1">
            Contact support if you need assistance with verification.
          </p>
        </div>
      </div>
    );
  };

  return (
    <div>
      {renderStatusContent()}
    </div>
  );
};

export default VerificationStatus;