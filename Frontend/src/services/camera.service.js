import axios from 'axios'
import { useAuthStore } from '@/stores/auth.store'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

export const cameraService = {
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
      
      console.log('Fetching cameras with config:', config)
      const response = await axios.get(`${API_URL}/cameras`, config)
      console.log('Cameras API response:', response)
      return response.data
    } catch (error) {
      console.error('Error in getAll cameras:', error)
      throw error
    }
  },
  
  async getById(id) {
    const authStore = useAuthStore()
    const token = await authStore.getToken()
    
    const response = await axios.get(`${API_URL}/cameras/${id}`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    return response.data
  },
  
  async create(camera) {
    try {
      const authStore = useAuthStore()
      const token = await authStore.getToken()
      
      if (!token) {
        console.error('No authentication token available')
        throw new Error('Authentication required')
      }
      
      console.log('Creating camera with data:', camera)
      console.log('Using token:', token ? 'Token exists' : 'No token')
      
      const response = await axios.post(`${API_URL}/cameras`, camera, {
        headers: { Authorization: `Bearer ${token}` }
      })
      
      console.log('Camera created successfully:', response.data)
      return response.data
    } catch (error) {
      console.error('Error creating camera:', error)
      if (error.response) {
        console.error('Response data:', error.response.data)
        console.error('Response status:', error.response.status)
        console.error('Response headers:', error.response.headers)
      } else if (error.request) {
        console.error('No response received:', error.request)
      } else {
        console.error('Error message:', error.message)
      }
      throw error
    }
  },
  
  async update(id, camera) {
    const authStore = useAuthStore()
    const token = await authStore.getToken()
    
    const response = await axios.put(`${API_URL}/cameras/${id}`, camera, {
      headers: { Authorization: `Bearer ${token}` }
    })
    return response.data
  },
  
  async delete(id) {
    const authStore = useAuthStore()
    const token = await authStore.getToken()
    
    const response = await axios.delete(`${API_URL}/cameras/${id}`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    return response.data
  }
}
