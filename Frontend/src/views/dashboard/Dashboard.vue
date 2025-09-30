<script setup>
import DateTimeFormat from '@/components/DateTimeFormat.vue'
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useImagesStore } from '@/stores/images.store'
import { useAuthStore } from '@/stores/auth.store'
import { CIcon } from '@coreui/icons-vue'
import { cilCamera } from '@coreui/icons'
import {
  CSpinner,
  CAlert,
  CCardImage,
  CCardTitle,
  CCardText,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CTable,
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell
} from '@coreui/vue'

// Initialize stores and router
const imagesStore = useImagesStore()
const authStore = useAuthStore()
const router = useRouter()

// State
const loading = ref(false)
const error = ref(null)
const selectedImage = ref(null)
const showImageModal = ref(false)

// Fetch images when component mounts
onMounted(async () => {
  if (!authStore.isAuthenticated) {
    router.push('/pages/login')
    return
  }
  
  loading.value = true
  try {
    await imagesStore.fetchImages()
  } catch (err) {
    error.value = 'Failed to load images. Please try again.'
    console.error('Error loading images:', err)
  } finally {
    loading.value = false
  }
})

// Handle image selection
function selectImage(image) {
  selectedImage.value = image
  showImageModal.value = true
}

// Import date formatting utilities
import { formatDateTime } from '@/utils/dateUtils'
</script>

<template>
  <div>
    <CRow>
      <CCol :md="12">
        <CCard class="mb-4">
          <CCardHeader>
            <h4 class="card-title mb-0">TrapCam Dashboard</h4>
            <div class="small text-body-secondary">View and manage your camera trap images</div>
          </CCardHeader>
          <CCardBody>
            <CAlert v-if="error" color="danger">
              {{ error }}
            </CAlert>
            
            <!-- Coming Soon Message -->
            <div class="text-center my-5">
              <h2>Coming Soon</h2>
              <p class="lead mt-3">The TrapCam Dashboard is being redesigned to provide you with better insights into your wildlife captures.</p>
              <p>Please check out the new <router-link to="/images">Images</router-link> section to browse your camera trap images.</p>
              
              <div class="coming-soon-graphic mt-5 mb-4">
                <CIcon :icon="cilCamera" size="xxl" />
                <div class="coming-soon-badge">Coming Soon</div>
              </div>
            </div>
          </CCardBody>
          <CCardFooter v-if="imagesStore.hasImages">
            <div class="d-flex justify-content-between align-items-center">
              <div>Total Images: {{ imagesStore.imagesCount }}</div>
              <CButton color="primary" variant="outline">
                <CIcon icon="cil-cloud-download" class="me-2" />
                Export Data
              </CButton>
            </div>
          </CCardFooter>
        </CCard>
      </CCol>
    </CRow>
    
    <!-- Image Modal -->
    <CModal 
      :visible="showImageModal" 
      @close="showImageModal = false"
      size="lg"
      backdrop="static"
    >
      <CModalHeader>
        <CModalTitle>{{ typeof selectedImage === 'string' ? 'Wildlife Image' : (selectedImage?.name || 'Image Details') }}</CModalTitle>
      </CModalHeader>
      <CModalBody>
        <div v-if="selectedImage" class="text-center">
          <img :src="typeof selectedImage === 'string' ? selectedImage : (selectedImage.url || 'https://via.placeholder.com/800x600?text=TrapCam+Image')" 
               class="img-fluid mb-3" 
               alt="TrapCam Image" />
          
          <CTable v-if="typeof selectedImage !== 'string'" responsive>
            <CTableBody>
              <CTableRow>
                <CTableHeaderCell>File Name</CTableHeaderCell>
                <CTableDataCell>{{ selectedImage.name || 'Unknown' }}</CTableDataCell>
              </CTableRow>
              <CTableRow>
                <CTableHeaderCell>Capture Date</CTableHeaderCell>
                <CTableDataCell><DateTimeFormat :value="selectedImage.captureDate" type="dateTime" /></CTableDataCell>
              </CTableRow>
              <CTableRow v-if="selectedImage.location">
                <CTableHeaderCell>Location</CTableHeaderCell>
                <CTableDataCell>{{ selectedImage.location }}</CTableDataCell>
              </CTableRow>
              <CTableRow v-if="selectedImage.species">
                <CTableHeaderCell>Species</CTableHeaderCell>
                <CTableDataCell>{{ selectedImage.species }}</CTableDataCell>
              </CTableRow>
              <CTableRow v-if="selectedImage.confidence">
                <CTableHeaderCell>Confidence</CTableHeaderCell>
                <CTableDataCell>{{ selectedImage.confidence }}%</CTableDataCell>
              </CTableRow>
            </CTableBody>
          </CTable>
          <div v-else class="text-center mt-3">
            <h5>Wildlife Image</h5>
            <p>This is a sample wildlife image from the TrapCam system.</p>
          </div>
        </div>
      </CModalBody>
      <CModalFooter>
        <CButton color="secondary" @click="showImageModal = false">Close</CButton>
        <CButton color="primary">Download</CButton>
      </CModalFooter>
    </CModal>
  </div>
</template>

<style scoped>
.image-thumbnail {
  height: 200px;
  object-fit: cover;
  cursor: pointer;
  transition: transform 0.2s;
}

.image-thumbnail:hover {
  transform: scale(1.03);
}

.coming-soon-graphic {
  position: relative;
  display: inline-block;
  padding: 2rem;
  background-color: #f8f9fa;
  border-radius: 1rem;
}

.coming-soon-badge {
  position: absolute;
  top: -10px;
  right: -10px;
  background-color: var(--cui-primary);
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 1rem;
  font-weight: bold;
  transform: rotate(5deg);
}
</style>
