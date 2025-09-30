import axios from 'axios'
import { getAuthHeader } from './authHeader'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

export const cameraService = {
  /**
   * Get all cameras for the current user
   * @returns {Promise<Array>} Array of camera objects
   */
  async getCameras() {
    const response = await axios.get(`${API_URL}/cameras`, {
      headers: getAuthHeader()
    })
    return response.data
  },

  /**
   * Get a specific camera by ID
   * @param {string} id Camera ID
   * @returns {Promise<Object>} Camera object
   */
  async getCamera(id) {
    const response = await axios.get(`${API_URL}/cameras/${id}`, {
      headers: getAuthHeader()
    })
    return response.data
  },

  /**
   * Create a new camera
   * @param {Object} camera Camera object
   * @returns {Promise<Object>} Created camera object
   */
  async createCamera(camera) {
    const response = await axios.post(`${API_URL}/cameras`, camera, {
      headers: getAuthHeader()
    })
    return response.data
  },

  /**
   * Update an existing camera
   * @param {string} id Camera ID
   * @param {Object} camera Camera object
   * @returns {Promise<Object>} Updated camera object
   */
  async updateCamera(id, camera) {
    const response = await axios.put(`${API_URL}/cameras/${id}`, camera, {
      headers: getAuthHeader()
    })
    return response.data
  },

  /**
   * Delete a camera
   * @param {string} id Camera ID
   * @returns {Promise<Object>} Response with success message
   */
  async deleteCamera(id) {
    const response = await axios.delete(`${API_URL}/cameras/${id}`, {
      headers: getAuthHeader()
    })
    return response.data
  }
}
