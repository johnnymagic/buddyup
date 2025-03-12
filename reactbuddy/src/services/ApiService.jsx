/**
 * API service for making HTTP requests to the backend
 */
class ApiService {
  constructor() {
    this.baseUrl = import.meta.env.REACT_APP_API_URL || 'https://api.buddyup.com';
    this.headers = {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    };
    this.token = null;
  }

  /**
   * Set the authentication token for API requests
   * @param {string} token - JWT token
   */
  setAuthToken(token) {
    this.token = token;
    
    if (token) {
      this.headers['Authorization'] = `Bearer ${token}`;
    } else {
      delete this.headers['Authorization'];
    }
  }

  /**
   * Clear the authentication token
   */
  clearAuthToken() {
    this.token = null;
    delete this.headers['Authorization'];
  }

  /**
   * Make a GET request
   * @param {string} endpoint - API endpoint
   * @param {Object} params - Query parameters
   * @returns {Promise<any>} Response data
   */
  async get(endpoint, params = {}) {
    const url = new URL(`${this.baseUrl}${endpoint}`);
    
    // Add query parameters
    Object.keys(params).forEach(key => {
      if (params[key] !== undefined && params[key] !== null) {
        url.searchParams.append(key, params[key]);
      }
    });
    
    try {
      const response = await fetch(url.toString(), {
        method: 'GET',
        headers: this.headers
      });
      
      return this._handleResponse(response);
    } catch (error) {
      return this._handleError(error);
    }
  }

  /**
   * Make a POST request
   * @param {string} endpoint - API endpoint
   * @param {Object} data - Request body
   * @returns {Promise<any>} Response data
   */
  async post(endpoint, data = {}) {
    try {
      const response = await fetch(`${this.baseUrl}${endpoint}`, {
        method: 'POST',
        headers: this.headers,
        body: JSON.stringify(data)
      });
      
      return this._handleResponse(response);
    } catch (error) {
      return this._handleError(error);
    }
  }

  /**
   * Make a PUT request
   * @param {string} endpoint - API endpoint
   * @param {Object} data - Request body
   * @returns {Promise<any>} Response data
   */
  async put(endpoint, data = {}) {
    try {
      const response = await fetch(`${this.baseUrl}${endpoint}`, {
        method: 'PUT',
        headers: this.headers,
        body: JSON.stringify(data)
      });
      
      return this._handleResponse(response);
    } catch (error) {
      return this._handleError(error);
    }
  }

  /**
   * Make a DELETE request
   * @param {string} endpoint - API endpoint
   * @returns {Promise<any>} Response data
   */
  async delete(endpoint) {
    try {
      const response = await fetch(`${this.baseUrl}${endpoint}`, {
        method: 'DELETE',
        headers: this.headers
      });
      
      return this._handleResponse(response);
    } catch (error) {
      return this._handleError(error);
    }
  }

  /**
   * Handle API response
   * @private
   * @param {Response} response - Fetch Response object
   * @returns {Promise<any>} Parsed response data
   */
  async _handleResponse(response) {
    const contentType = response.headers.get('content-type');
    const isJson = contentType && contentType.includes('application/json');
    const data = isJson ? await response.json() : await response.text();
    
    if (!response.ok) {
      // If the server returned an error with additional info
      const error = {
        status: response.status,
        statusText: response.statusText,
        data
      };
      
      throw error;
    }
    
    return data;
  }

  /**
   * Handle API errors
   * @private
   * @param {Error} error - Error object
   * @throws {Error} Rethrows the error with additional info
   */
  _handleError(error) {
    console.error('API Error:', error);
    
    // Enhance the error object with additional debugging info
    if (error.status) {
      // This is an HTTP error from _handleResponse
      const message = typeof error.data === 'object' 
        ? error.data.message || error.statusText
        : error.statusText;
      
      throw new Error(message);
    }
    
    // Network or other error
    throw error;
  }
}

// Create and export a singleton instance
export const api = new ApiService();