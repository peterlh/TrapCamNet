import axios from 'axios'
import { useAuthStore } from '@/stores/auth.store'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

export const emailArchiveService = {
  /**
   * Get all email archives for a camera
   * @param {string} cameraId - The ID of the camera
   * @returns {Promise<Array>} - List of email archives
   */
  async getAll(cameraId) {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      const config = {
        headers: { Authorization: `Bearer ${token}` }
      }
      
      const response = await axios.get(`${API_URL}/cameras/${cameraId}/EmailArchive`, config)
      return response.data
    } catch (error) {
      console.error('Error fetching email archives:', error)
      throw error
    }
  },
  
  /**
   * Get email content for a specific email archive
   * @param {string} cameraId - The ID of the camera
   * @param {string} emailId - The ID of the email archive
   * @returns {Promise<string>} - HTML content of the email
   */
  async getContent(cameraId, emailId) {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      const config = {
        headers: { Authorization: `Bearer ${token}` },
        responseType: 'text'
      }
      
      const response = await axios.get(
        `${API_URL}/cameras/${cameraId}/EmailArchive/${emailId}/Content`, 
        config
      )
      return response.data
    } catch (error) {
      console.error('Error fetching email content:', error)
      throw error
    }
  },
  
  /**
   * Get the image URL for a specific email archive
   * @param {string} cameraId - The ID of the camera
   * @param {string} emailId - The ID of the email archive
   * @returns {string} - URL to the email image
   */
  getImageUrl(cameraId, emailId) {
    return `${API_URL}/cameras/${cameraId}/EmailArchive/${emailId}/Image`
  },
  
  /**
   * Fetch image data with authentication and return as a blob URL
   * @param {string} cameraId - The ID of the camera
   * @param {string} emailId - The ID of the email archive
   * @returns {Promise<string>} - Blob URL for the image that can be used in img tags
   */
  async fetchImageWithAuth(cameraId, emailId) {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      const config = {
        headers: { Authorization: `Bearer ${token}` },
        responseType: 'blob'
      }
      
      const response = await axios.get(
        `${API_URL}/cameras/${cameraId}/EmailArchive/${emailId}/Image`, 
        config
      )
      
      // Create a blob URL from the response data
      return URL.createObjectURL(response.data)
    } catch (error) {
      console.error('Error fetching image with authentication:', error)
      throw error
    }
  }
}
