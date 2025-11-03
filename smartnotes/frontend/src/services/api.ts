import axios, { AxiosInstance, AxiosError } from 'axios';
import { APIError } from '../types';

// Create axios instance with default configuration
const api: AxiosInstance = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, // 10 seconds
});

// Request interceptor
api.interceptors.request.use(
  (config) => {
    // Add timestamp to prevent caching
    config.params = {
      ...config.params,
      _t: new Date().getTime(),
    };
    
    // Add any auth headers here if needed
    // const token = localStorage.getItem('authToken');
    // if (token) {
    //   config.headers.Authorization = `Bearer ${token}`;
    // }
    
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor
api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error: AxiosError) => {
    const apiError: APIError = {
      message: 'An unexpected error occurred',
      status: error.response?.status,
    };

    if (error.response?.data) {
      const errorData = error.response.data as any;
      apiError.message = errorData.message || errorData.title || apiError.message;
      apiError.traceId = errorData.traceId;
    } else if (error.message) {
      apiError.message = error.message;
    }

    // Handle specific status codes
    switch (error.response?.status) {
      case 400:
        apiError.message = apiError.message || 'Invalid request data';
        break;
      case 401:
        apiError.message = 'Unauthorized access';
        // Handle auth redirect here if needed
        break;
      case 403:
        apiError.message = 'Access forbidden';
        break;
      case 404:
        apiError.message = 'Resource not found';
        break;
      case 409:
        apiError.message = 'Resource conflict';
        break;
      case 422:
        apiError.message = 'Validation failed';
        break;
      case 500:
        apiError.message = 'Server error occurred';
        break;
      case 503:
        apiError.message = 'Service temporarily unavailable';
        break;
      default:
        if (error.code === 'ECONNABORTED') {
          apiError.message = 'Request timeout';
        } else if (error.code === 'ERR_NETWORK') {
          apiError.message = 'Network error - please check your connection';
        }
    }

    return Promise.reject(apiError);
  }
);

export default api;