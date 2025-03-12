import React, { useState } from 'react';
import { useProfile } from '../hooks/useProfile';
import Button from '../components/common/Button';
import VerificationStatus from '../components/profile/VerificationStatus';

/**
 * Verification page component for identity verification
 */
const Verification = () => {
  const { verificationStatus, initiateVerification, loading, error } = useProfile();
  const [verificationMethod, setVerificationMethod] = useState('clear');
  const [agreementChecked, setAgreementChecked] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);

  const handleStartVerification = async () => {
    try {
      await initiateVerification('Identity', verificationMethod);
      setShowSuccess(true);
      
      // Reset success message after a delay
      setTimeout(() => {
        setShowSuccess(false);
      }, 5000);
    } catch (err) {
      console.error('Error initiating verification', err);
    }
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-6">Identity Verification</h1>
      
      <div className="bg-white rounded-lg shadow-md p-6 mb-8">
        <h2 className="text-xl font-semibold mb-4">Why Verify Your Identity?</h2>
        
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
          <div className="bg-blue-50 p-4 rounded-md">
            <div className="flex items-center mb-2">
              <svg className="h-6 w-6 text-blue-600 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
              <h3 className="font-medium">Safety</h3>
            </div>
            <p className="text-sm text-gray-600">
              Verification helps ensure that everyone in our community is who they say they are.
            </p>
          </div>
          
          <div className="bg-blue-50 p-4 rounded-md">
            <div className="flex items-center mb-2">
              <svg className="h-6 w-6 text-blue-600 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M17 8h2a2 2 0 012 2v6a2 2 0 01-2 2h-2v4l-4-4H9a1.994 1.994 0 01-1.414-.586m0 0L11 14h4a2 2 0 002-2V6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2v4l.586-.586z" />
              </svg>
              <h3 className="font-medium">Trust</h3>
            </div>
            <p className="text-sm text-gray-600">
              Verified users receive a special badge, making it easier to build trust with potential workout buddies.
            </p>
          </div>
          
          <div className="bg-blue-50 p-4 rounded-md">
            <div className="flex items-center mb-2">
              <svg className="h-6 w-6 text-blue-600 mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M14 10h4.764a2 2 0 011.789 2.894l-3.5 7A2 2 0 0115.263 21h-4.017c-.163 0-.326-.02-.485-.06L7 20m7-10V5a2 2 0 00-2-2h-.095c-.5 0-.905.405-.905.905 0 .714-.211 1.412-.608 2.006L7 11v9m7-10h-2M7 20H5a2 2 0 01-2-2v-6a2 2 0 012-2h2.5" />
              </svg>
              <h3 className="font-medium">Priority Matching</h3>
            </div>
            <p className="text-sm text-gray-600">
              Verified users are prioritized in search results and matching algorithms.
            </p>
          </div>
        </div>
        
        <hr className="my-6" />
        
        <h2 className="text-xl font-semibold mb-4">Current Verification Status</h2>
        <VerificationStatus />
      </div>
      
      {verificationStatus?.status !== 'Verified' && (
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold mb-4">Start Verification Process</h2>
          
          {showSuccess ? (
            <div className="bg-green-50 border-l-4 border-green-400 p-4 mb-6">
              <div className="flex">
                <div className="flex-shrink-0">
                  <svg className="h-5 w-5 text-green-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <p className="text-sm text-green-700">
                    Verification process initiated successfully! Please check your email for next steps.
                  </p>
                </div>
              </div>
            </div>
          ) : error ? (
            <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-6">
              <div className="flex">
                <div className="flex-shrink-0">
                  <svg className="h-5 w-5 text-red-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <p className="text-sm text-red-700">
                    {error}
                  </p>
                </div>
              </div>
            </div>
          ) : null}
          
          <div className="mb-6">
            <label className="block text-gray-700 font-medium mb-2">
              Verification Method
            </label>
            <div className="grid grid-cols-1 gap-4">
              <div className={`border rounded-lg p-4 ${
                verificationMethod === 'clear' ? 'border-blue-500 bg-blue-50' : 'border-gray-200'
              }`}>
                <label className="flex items-start cursor-pointer">
                  <input
                    type="radio"
                    className="mt-1 h-4 w-4 text-blue-600"
                    value="clear"
                    checked={verificationMethod === 'clear'}
                    onChange={() => setVerificationMethod('clear')}
                  />
                  <div className="ml-3">
                    <span className="block font-medium">CLEAR Identity Verification</span>
                    <span className="block text-sm text-gray-500 mt-1">
                      Use your CLEAR account to verify your identity quickly and securely.
                    </span>
                  </div>
                </label>
              </div>
              
              <div className={`border rounded-lg p-4 ${
                verificationMethod === 'id' ? 'border-blue-500 bg-blue-50' : 'border-gray-200 opacity-50'
              }`}>
                <label className="flex items-start cursor-pointer">
                  <input
                    type="radio"
                    className="mt-1 h-4 w-4 text-blue-600"
                    value="id"
                    checked={verificationMethod === 'id'}
                    onChange={() => setVerificationMethod('id')}
                    disabled
                  />
                  <div className="ml-3">
                    <span className="block font-medium">ID Document Verification</span>
                    <span className="block text-sm text-gray-500 mt-1">
                      Upload your government-issued ID to verify your identity.
                      <span className="text-xs text-blue-600 ml-2">(Coming soon)</span>
                    </span>
                  </div>
                </label>
              </div>
            </div>
          </div>
          
          <div className="mb-6">
            <label className="flex items-start">
              <input
                type="checkbox"
                className="mt-1 h-4 w-4 text-blue-600 rounded"
                checked={agreementChecked}
                onChange={() => setAgreementChecked(!agreementChecked)}
              />
              <span className="ml-3 text-sm text-gray-600">
                I understand that my identity information will be verified by a third-party service and stored securely. This helps ensure the safety of all Buddy Up users.
              </span>
            </label>
          </div>
          
          <Button
            onClick={handleStartVerification}
            disabled={!agreementChecked || loading || verificationStatus?.status === 'Pending'}
            className="bg-blue-600 hover:bg-blue-700 text-white"
          >
            {loading ? 'Processing...' : 'Start Verification Process'}
          </Button>
        </div>
      )}
    </div>
  );
};

export default Verification;