import axios from 'axios'
import { useAuthStore } from '@/stores/auth.store'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

export const notificationService = {
  async getDevices() {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      const response = await axios.get(`${API_URL}/notifications/devices`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      return response.data
    } catch (error) {
      console.error('Error getting notification devices:', error)
      throw error
    }
  },

  async registerDevice(device) {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      const response = await axios.post(`${API_URL}/notifications/devices`, device, {
        headers: { Authorization: `Bearer ${token}` }
      })
      return response.data
    } catch (error) {
      console.error('Error registering notification device:', error)
      throw error
    }
  },

  async updateDeviceSubscriptions(deviceId, cameraIds, notifyOnlyOnAnimalDetection) {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      const response = await axios.put(
        `${API_URL}/notifications/devices/${deviceId}/subscriptions`, 
        { cameraIds, notifyOnlyOnAnimalDetection }, 
        { headers: { Authorization: `Bearer ${token}` } }
      )
      return response.data
    } catch (error) {
      console.error('Error updating device subscriptions:', error)
      throw error
    }
  },

  async deleteDevice(deviceId) {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      const response = await axios.delete(`${API_URL}/notifications/devices/${deviceId}`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      return response.data
    } catch (error) {
      console.error('Error deleting notification device:', error)
      throw error
    }
  }
}
