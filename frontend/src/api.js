import axios from 'axios';

const API = axios.create({
  baseURL: 'https://localhost:7000/api', // Yahan apne .NET backend ka URL dalein
  headers: {
    'Content-Type': 'application/json',
  },
});

// Har request ke sath JWT token automatically bhejne ke liye interceptor
API.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, (error) => {
  return Promise.reject(error);
});

export default API;