import axios from 'axios'
import { getAuthHeader } from './authHeader'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

export const notificationService = {
  /**
   * Get all devices for the current user
   * @returns {Promise<Array>} Array of device objects
   */
  async getDevices() {
    const response = await axios.get(`${API_URL}/notifications/devices`, {
      headers: getAuthHeader()
    })
    return response.data
  },

  /**
   * Register a new device for push notifications
   * @param {Object} device Device object with name and fcmToken
   * @returns {Promise<Object>} Response with device ID
   */
  async registerDevice(device) {
    const response = await axios.post(`${API_URL}/notifications/devices`, device, {
      headers: getAuthHeader()
    })
    return response.data
  },

  /**
   * Update device camera subscriptions
   * @param {string} deviceId Device ID
   * @param {Array} cameraIds Array of camera IDs
   * @returns {Promise<Object>} Response with success message
   */
  async updateDeviceSubscriptions(deviceId, cameraIds) {
    const response = await axios.put(
      `${API_URL}/notifications/devices/${deviceId}/subscriptions`, 
      { cameraIds }, 
      { headers: getAuthHeader() }
    )
    return response.data
  },

  /**
   * Delete a device
   * @param {string} deviceId Device ID
   * @returns {Promise<Object>} Response with success message
   */
  async deleteDevice(deviceId) {
    const response = await axios.delete(`${API_URL}/notifications/devices/${deviceId}`, {
      headers: getAuthHeader()
    })
    return response.data
  }
}
