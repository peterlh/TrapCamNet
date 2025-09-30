<template>
  <div>
    <CRow>
      <CCol :md="12">
        <CCard class="mb-4">
          <CCardHeader>
            <h4 class="card-title mb-0">Image listing</h4>
            <div class="small text-body-secondary">
              {{ imagesCount }} images across {{ camerasCount }} cameras
            </div>
          </CCardHeader>
          <CCardBody>
            <CAlert v-if="error" color="danger">
              {{ error }}
            </CAlert>
            
            <!-- Loading spinner -->
            <div v-if="loading" class="text-center my-5">
              <CSpinner color="primary" />
              <div class="mt-3">Loading images...</div>
            </div>
            
            <!-- No images message -->
            <CAlert v-else-if="!imagesStore.hasImages" color="info">
              No images found. New images will appear here when they are captured by your trap cameras.
            </CAlert>
            
            <!-- Page size selector -->
            <div v-else class="d-flex justify-content-between align-items-center mb-4">
              <div>
                <CFormLabel for="pageSize">Items per page:</CFormLabel>
                <CFormSelect
                  id="pageSize"
                  v-model="pageSize"
                  :options="[
                    { label: '12', value: 12 },
                    { label: '24', value: 24 },
                    { label: '48', value: 48 },
                    { label: '96', value: 96 }
                  ]"
                  class="form-select-sm d-inline-block w-auto ms-2"
                  @change="handlePageSizeChange"
                />
              </div>
            </div>
            
            <!-- Images grid -->
            <CRow v-if="imagesStore.hasImages" :xs="{ cols: 1 }" :sm="{ cols: 2 }" :md="{ cols: 3 }" :xl="{ cols: 4 }" class="g-4">
              <CCol v-for="(image, index) in imagesStore.images" :key="image.id || index">
                <CCard class="h-100">
                  <CCardImage 
                    :src="getImageUrl(image.url) || 'https://via.placeholder.com/300x200?text=TrapCam+Image'" 
                    class="card-img-top image-thumbnail" 
                    @click="selectImage(image)" 
                  />
                  <CCardBody>
                    <CCardTitle><DateTimeFormat :value="image.imageDate" type="dateTime" /></CCardTitle>
                    <CCardText>
                      <div v-if="image.location"><strong>Location:</strong> {{ image.location }}</div>
                      <div>
                        <strong>Animals:</strong> <span v-if="image.animals && image.animals.length">{{ formatAnimals(image.animals) }}</span>
                      </div>
                    </CCardText>
                    <CButton color="primary" size="sm" @click="selectImage(image)">View Details</CButton>
                  </CCardBody>
                </CCard>
              </CCol>
            </CRow>
            
            <!-- Pagination -->
            <div v-if="imagesStore.hasImages && totalPages > 1" class="d-flex justify-content-center mt-4">
              <CPagination aria-label="Page navigation">
                <CPaginationItem 
                  :disabled="currentPage === 1"
                  @click="changePage(currentPage - 1)"
                >
                  Previous
                </CPaginationItem>
                
                <CPaginationItem 
                  v-for="page in displayedPages" 
                  :key="page"
                  :active="page === currentPage"
                  @click="changePage(page)"
                >
                  {{ page }}
                </CPaginationItem>
                
                <CPaginationItem 
                  :disabled="currentPage === totalPages"
                  @click="changePage(currentPage + 1)"
                >
                  Next
                </CPaginationItem>
              </CPagination>
            </div>
          </CCardBody>
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
        <CModalTitle>{{ selectedImage?.name || 'Image Details' }}</CModalTitle>
      </CModalHeader>
      <CModalBody>
        <div v-if="selectedImage" class="text-center">
          <img :src="getImageUrl(selectedImage.url) || 'https://via.placeholder.com/800x600?text=TrapCam+Image'" 
               class="img-fluid mb-3" 
               alt="TrapCam Image" />
          
          <CTable responsive>
            <CTableBody>
              <CTableRow>
                <CTableHeaderCell>File Name</CTableHeaderCell>
                <CTableDataCell>{{ selectedImage.name || 'Unknown' }}</CTableDataCell>
              </CTableRow>
              <CTableRow>
                <CTableHeaderCell>Capture Date</CTableHeaderCell>
                <CTableDataCell><DateTimeFormat :value="selectedImage.imageDate" type="dateTime" /></CTableDataCell>
              </CTableRow>
              <CTableRow v-if="selectedImage.location">
                <CTableHeaderCell>Location</CTableHeaderCell>
                <CTableDataCell>{{ selectedImage.location }}</CTableDataCell>
              </CTableRow>
              <CTableRow v-if="selectedImage.animals && selectedImage.animals.length">
                <CTableHeaderCell>Animals</CTableHeaderCell>
                <CTableDataCell>{{ formatAnimals(selectedImage.animals) }}</CTableDataCell>
              </CTableRow>
              <CTableRow v-if="selectedImage.cameraName">
                <CTableHeaderCell>Camera</CTableHeaderCell>
                <CTableDataCell>{{ selectedImage.cameraName }}</CTableDataCell>
              </CTableRow>
            </CTableBody>
          </CTable>
        </div>
      </CModalBody>
      <CModalFooter>
        <CButton color="secondary" @click="showImageModal = false">Close</CButton>
        <CButton color="primary">Download</CButton>
      </CModalFooter>
    </CModal>
  </div>
</template>

<script setup>
import DateTimeFormat from '@/components/DateTimeFormat.vue'
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useImagesStore } from '@/stores/images.store'
import { useAuthStore } from '@/stores/auth.store'
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
  CTableDataCell,
  CFormSelect,
  CFormLabel,
  CPagination,
  CPaginationItem
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
const currentPage = ref(1)
const pageSize = ref(12)

// Computed properties
const totalImages = computed(() => imagesStore.totalCount)
const totalPages = computed(() => imagesStore.totalPages)
const camerasCount = computed(() => imagesStore.camerasCount)
const imagesCount = computed(() => imagesStore.totalCount)

// Calculate which pages to display in pagination
const displayedPages = computed(() => {
  const total = totalPages.value
  const current = currentPage.value
  
  if (total <= 5) {
    // If 5 or fewer pages, show all
    return Array.from({ length: total }, (_, i) => i + 1)
  }
  
  // Always include first, last, current, and one on either side of current
  const pages = [1, total]
  
  for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) {
    pages.push(i)
  }
  
  // Sort and deduplicate
  return [...new Set(pages)].sort((a, b) => a - b)
})

// Fetch images when component mounts
onMounted(async () => {
  if (!authStore.isAuthenticated) {
    router.push('/pages/login')
    return
  }
  
  await loadImages()
})

// Watch for changes in page or page size
watch([currentPage, pageSize], async () => {
  await loadImages()
})

// Load images with pagination
async function loadImages() {
  loading.value = true
  try {
    await imagesStore.fetchImagesWithDetails(currentPage.value, pageSize.value)
  } catch (err) {
    error.value = 'Failed to load images. Please try again.'
    console.error('Error loading images:', err)
  } finally {
    loading.value = false
  }
}

// Methods
function selectImage(image) {
  selectedImage.value = image
  showImageModal.value = true
}

function changePage(page) {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    // Scroll to top of the page
    window.scrollTo(0, 0)
    // loadImages will be triggered by the watcher on currentPage
  }
}

function handlePageSizeChange() {
  // Reset to first page when page size changes
  currentPage.value = 1
}

function formatAnimals(animals) {
  if (!animals || !animals.length) return 'None'
  
  // Get unique animal names
  const uniqueAnimals = [...new Set(animals.map(animal => animal.commonName || 'Unknown'))]
  
  if (uniqueAnimals.length <= 2) {
    return uniqueAnimals.join(', ')
  } else {
    return `${uniqueAnimals.slice(0, 2).join(', ')} +${uniqueAnimals.length - 2} more`
  }
}

function getImageUrl(url) {
  if (!url) return null
  
  // The backend should now handle hostname translation and presigned URLs
  // This function is kept for future enhancements if needed
  return url
}
</script>

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

.pagination {
  cursor: pointer;
}
</style>
