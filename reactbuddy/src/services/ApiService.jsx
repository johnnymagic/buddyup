/**
 * API service for making HTTP requests to the backend
 */
class ApiService {
  constructor() {
    this.baseUrl = import.meta.env.VITE_API_URL;
    this.headers = {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    };
    
    // Load token from localStorage on initialization
    this.token = localStorage.getItem('auth_token');
    if (this.token) {
      this.headers['Authorization'] = `Bearer ${this.token}`;
    }
  }

  /**
   * Set the authentication token for API requests
   * @param {string} token - JWT token
   */
  setAuthToken(token) {
    this.token = token;
    
    if (token) {
      // Store in localStorage for persistence
      localStorage.setItem('auth_token', token);
      this.headers['Authorization'] = `Bearer ${token}`;

    } else {
      localStorage.removeItem('auth_token');
      delete this.headers['Authorization'];

    }
  }

  /**
   * Clear the authentication token
   */
  clearAuthToken() {
    this.token = null;
    localStorage.removeItem('auth_token');
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
        headers: this.headers,
        credentials: 'include' // For CORS with credentials
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
        body: JSON.stringify(data),
        credentials: 'include' // For CORS with credentials
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

      // Refresh token from localStorage in case it was updated elsewhere
      const storedToken = localStorage.getItem('auth_token');
      if (storedToken && storedToken !== this.token) {
        this.setAuthToken(storedToken);
      }
      
      const response = await fetch(`${this.baseUrl}${endpoint}`, {
        method: 'PUT',
        headers: this.headers,
        body: JSON.stringify(data),
        credentials: 'include' // For CORS with credentials
      });
      
      return this._handleResponse(response);
    } catch (error) {
      console.log('Error in ApiService.put:', error);
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
        headers: this.headers,
        credentials: 'include' // For CORS with credentials
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

    
    // Log all headers for debugging
    const headers = {};
    response.headers.forEach((value, key) => {
      headers[key] = value;
    });

    
    const contentType = response.headers.get('content-type');
    const isJson = contentType && contentType.includes('application/json');
    
    // Handle 401 Unauthorized specifically
    if (response.status === 401) {
      console.error('401 Unauthorized - Token may be invalid or expired');
      // Consider clearing the token or redirecting to login
      // this.clearAuthToken();
      // window.location.href = '/login';
    }
    
    let data;
    try {
      data = isJson ? await response.json() : await response.text();

    } catch (err) {
      console.error('Error parsing response:', err);
      data = { error: 'Failed to parse response' };
    }
    
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