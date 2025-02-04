import axios from 'axios';

// Set the default API URL
axios.defaults.baseURL = process.env.REACT_APP_API_URL;

// Add an interceptor to log errors
axios.interceptors.response.use(
  response => response,
  error => {
    console.error('API error:', error); // Log the error to the console
    return Promise.reject(error); // Reject the promise to handle it in the calling code
  }
);

// Assign the API methods to a variable
const api = {
  getTasks: async () => {
    const result = await axios.get('/tasks');    
    return result.data;
  },

  addTask: async (name) => {
    const result = await axios.post('/tasks', { name, isComplete: false });
    return result.data;
  },

  setCompleted: async (id, isComplete) => {
    const result = await axios.put(`/tasks/${id}`, { isComplete });
    return result.data;
  },

  deleteTask: async (id) => {
    await axios.delete(`/tasks/${id}`);
  }
};

// Export the api object as the default export
export default api;
