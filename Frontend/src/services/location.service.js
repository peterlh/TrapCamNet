import axios from 'axios'
import { useAuthStore } from '@/stores/auth.store'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

export const locationService = {
  async getAll(search = '') {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      const config = {
        headers: { Authorization: `Bearer ${token}` },
        params: {}
      }
      
      if (search) {
        config.params.search = search
      }
      
      console.log('Fetching locations with config:', config)
      const response = await axios.get(`${API_URL}/locations`, config)
      console.log('Locations API response:', response)
      return response.data
    } catch (error) {
      console.error('Error in getAll locations:', error)
      throw error
    }
  },
  
  async getById(id) {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      const response = await axios.get(`${API_URL}/locations/${id}`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      return response.data
    } catch (error) {
      console.error('Error getting location by ID:', error)
      throw error
    }
  },
  
  async create(location) {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      console.log('Creating location with data:', location)
      
      const response = await axios.post(`${API_URL}/locations`, location, {
        headers: { Authorization: `Bearer ${token}` }
      })
      
      console.log('Location created successfully:', response.data)
      return response.data
    } catch (error) {
      console.error('Error creating location:', error)
      if (error.response) {
        console.error('Response data:', error.response.data)
        console.error('Response status:', error.response.status)
      }
      throw error
    }
  },
  
  async update(id, location) {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      console.log('Updating location with data:', location)
      
      const response = await axios.put(`${API_URL}/locations/${id}`, location, {
        headers: { Authorization: `Bearer ${token}` }
      })
      
      console.log('Location updated successfully')
      return response.data
    } catch (error) {
      console.error('Error updating location:', error)
      if (error.response) {
        console.error('Response data:', error.response.data)
        console.error('Response status:', error.response.status)
      }
      throw error
    }
  },
  
  async delete(id) {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      const response = await axios.delete(`${API_URL}/locations/${id}`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      
      console.log('Location deleted successfully')
      return response.data
    } catch (error) {
      console.error('Error deleting location:', error)
      if (error.response) {
        console.error('Response data:', error.response.data)
        console.error('Response status:', error.response.status)
      }
      throw error
    }
  }
}
