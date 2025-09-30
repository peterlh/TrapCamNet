<template>
  <div>
    <CCard class="mb-4">
      <CCardHeader>
        <strong>Camera Details: {{ camera.name }}</strong>
      </CCardHeader>
      <CCardBody>
        <!-- Loading indicator -->
        <div v-if="loading" class="text-center my-5">
          <CSpinner />
          <p class="mt-2">Loading camera details...</p>
        </div>
        
        <!-- Error message -->
        <CAlert v-if="error" color="danger">
          {{ error }}
        </CAlert>
        
        <div v-if="!loading && !error">
          <CRow>
            <CCol>
              <CButton color="secondary" @click="goBack" class="mb-3">
                <CIcon icon="cil-arrow-left" /> Back to Cameras
              </CButton>
            </CCol>
          </CRow>
          
          <CRow class="mb-4">
            <CCol md="6">
              <CCard>
                <CCardBody>
                  <h4>Camera Information</h4>
                  <div class="mb-3">
                    <strong>Name:</strong> {{ camera.name }}
                  </div>
                  <div class="mb-3">
                    <strong>Email Address:</strong> {{ camera.inboundEmailAddress }}
                  </div>
                  <div class="mb-3">
                    <strong>Battery Status:</strong>
                    <div v-if="camera.batteryInfo?.voltage" class="mt-1">
                      <CBadge :color="getBatteryVoltageColor(camera.batteryInfo.voltage)" class="ms-2">
                        {{ camera.batteryInfo.voltage }}V
                      </CBadge>
                    </div>
                    <div v-else class="mt-1">
                      <CBadge :color="getBatteryColor(camera.lastBatteryState)" class="ms-2">
                        {{ camera.lastBatteryState }}%
                      </CBadge>
                    </div>
                    <div v-if="camera.batteryInfo?.rawMatch" class="mt-1 small text-muted">
                      Raw match: {{ camera.batteryInfo.rawMatch }}
                    </div>
                  </div>
                  <div class="mb-3">
                    <strong>Location:</strong> {{ camera.currentLocation?.name || 'N/A' }}
                  </div>
                  <div class="mb-3">
                    <strong>Last Contact:</strong> 
                    <span v-if="camera.lastContact"><DateTimeFormat :value="camera.lastContact" type="dateTime" defaultText="Never" /></span>
                    <span v-else class="text-muted">Never</span>
                  </div>

                  <div class="mt-4">
                    <CButton color="primary" @click="editCamera" class="me-2">
                      <CIcon icon="cil-pencil" /> Edit Camera
                    </CButton>
                    <CButton color="info" @click="viewEmailArchive">
                      <CIcon icon="cil-envelope" /> Email Archive
                    </CButton>
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md="6">
              <CCard>
                <CCardBody>
                  <h4>Recent Images</h4>
                  <p v-if="!images.length" class="text-muted">No images available.</p>
                  <div class="image-grid">
                    <!-- Placeholder for future image gallery implementation -->
                    <div v-for="i in 3" :key="i" class="image-placeholder">
                      <div class="placeholder-content">
                        <CIcon icon="cil-image" size="xl" />
                        <span>Image placeholder</span>
                      </div>
                    </div>
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>
        </div>
      </CCardBody>
    </CCard>
    
    <!-- Edit Camera Modal -->
    <CModal 
      :visible="editModalVisible" 
      @close="closeEditModal" 
      title="Edit Camera"
    >
      <CModalBody>
        <CForm>
          <div class="mb-3">
            <CFormLabel for="cameraName">Camera Name</CFormLabel>
            <CFormInput id="cameraName" v-model="editForm.name" />
          </div>
          <div class="mb-3">
            <CFormLabel for="cameraEmail">Email Address</CFormLabel>
            <CFormInput id="cameraEmail" v-model="editForm.inboundEmailAddress" />
          </div>
          <div class="mb-3">
            <CFormLabel for="cameraLocation">Location</CFormLabel>
            <CFormSelect id="cameraLocation" v-model="editForm.locationId">
              <option value="">No Location</option>
              <option v-for="location in locations" :key="location.id" :value="location.id">
                {{ location.name }}
              </option>
            </CFormSelect>
          </div>

        </CForm>
      </CModalBody>
      <CModalFooter>
        <CButton color="secondary" @click="closeEditModal">
          Cancel
        </CButton>
        <CButton color="primary" @click="saveCamera">
          Save Changes
        </CButton>
      </CModalFooter>
    </CModal>
  </div>
</template>

<script>
import DateTimeFormat from '@/components/DateTimeFormat.vue'
import { ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from '@/composables/toast'
import apiClient from '@/services/api'

export default {
  name: 'CameraDetail',
  components: {
    DateTimeFormat
  },
  setup() {
    const route = useRoute()
    const router = useRouter()
    const { addToast } = useToast()
    
    const cameraId = ref(route.params.id)
    const camera = ref({})
    const images = ref([])
    const locations = ref([])
    const loading = ref(true)
    const error = ref(null)
    
    // Edit modal state
    const editModalVisible = ref(false)
    const editForm = reactive({
      name: '',
      inboundEmailAddress: '',
      locationId: '',

    })
    
    // Fetch camera details
    const fetchCameraDetails = async () => {
      loading.value = true
      error.value = null
      
      try {
        const response = await apiClient.get(`/api/cameras/${cameraId.value}`)
        camera.value = response.data
        
        // Also fetch locations for the edit form
        const locationsResponse = await apiClient.get('/api/locations')
        locations.value = locationsResponse.data
      } catch (err) {
        console.error('Error fetching camera details:', err)
        error.value = 'Failed to load camera details. Please try again later.'
        addToast({
          title: 'Error',
          message: 'Failed to load camera details',
          color: 'danger'
        })
      } finally {
        loading.value = false
      }
    }
    
    // Battery status color
    const getBatteryColor = (batteryLevel) => {
      if (batteryLevel >= 70) return 'success'
      if (batteryLevel >= 30) return 'warning'
      return 'danger'
    }
    
    const getBatteryVoltageColor = (voltage) => {
      if (voltage >= 12) return 'success'
      if (voltage >= 10) return 'warning'
      return 'danger'
    }
    
    // Using the centralized date formatting system instead of local formatDate function
    
    // Navigate back to cameras list
    const goBack = () => {
      router.push('/cameras')
    }
    
    // Open edit modal
    const editCamera = () => {
      // Populate form with current camera data
      editForm.name = camera.value.name
      editForm.inboundEmailAddress = camera.value.inboundEmailAddress
      editForm.locationId = camera.value.locationId || ''

      
      editModalVisible.value = true
    }
    
    // Close edit modal
    const closeEditModal = () => {
      editModalVisible.value = false
    }
    
    // Save camera changes
    const saveCamera = async () => {
      try {
        await apiClient.put(`/api/cameras/${cameraId.value}`, editForm)
        
        // Refresh camera data
        await fetchCameraDetails()
        
        // Close modal
        editModalVisible.value = false
        
        addToast({
          title: 'Success',
          message: 'Camera updated successfully',
          color: 'success'
        })
      } catch (err) {
        console.error('Error updating camera:', err)
        addToast({
          title: 'Error',
          message: 'Failed to update camera',
          color: 'danger'
        })
      }
    }
    
    // Navigate to email archive
    const viewEmailArchive = () => {
      router.push(`/cameras/${cameraId.value}/email-archive`)
    }
    
    // Load data on component mount
    onMounted(() => {
      fetchCameraDetails()
    })
    
    return {
      camera,
      images,
      locations,
      loading,
      error,
      editModalVisible,
      editForm,
      getBatteryColor,
      getBatteryVoltageColor,
      // formatDate removed in favor of DateTimeFormat component
      goBack,
      editCamera,
      closeEditModal,
      saveCamera,
      viewEmailArchive
    }
  }
}
</script>

<style scoped>
.image-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 1rem;
}

.image-placeholder {
  border: 1px dashed #ccc;
  border-radius: 4px;
  aspect-ratio: 4/3;
  display: flex;
  align-items: center;
  justify-content: center;
}

.placeholder-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  color: #6c757d;
}
</style>
