<template>
  <div>
    <CCard class="mb-4">
      <CCardHeader>
        <strong>Email Archive for {{ cameraName }}</strong>
      </CCardHeader>
      <CCardBody>
        <CRow>
          <CCol>
            <div class="d-flex gap-2 mb-3">
              <CButton color="primary" @click="goBack">
                <CIcon icon="cil-arrow-left" /> Back to Cameras
              </CButton>
              <CButton color="success" @click="refreshEmails">
                <CIcon icon="cil-reload" /> Refresh Emails
              </CButton>
            </div>
          </CCol>
        </CRow>


        This shows the last emails received by this camera. You can use this to ensure it is sending emails. 
        Also can be usefull if you are automatically forwarding emails from your email provider to this camera inbox, as this sometimes requires approval via link from email.
        <br/>
        <br/>
        
        <!-- Loading indicator -->
        <div v-if="loading" class="text-center my-5">
          <CSpinner />
          <p class="mt-2">Loading email archives...</p>
        </div>
        
        <!-- Error message -->
        <CAlert v-if="error" color="danger">
          {{ error }}
        </CAlert>
        
        <!-- No emails message -->
        <CAlert v-if="!loading && !error && emails.length === 0" color="info">
          No emails found for this camera.
        </CAlert>
        
        <!-- Email archive table -->
        <CTable v-if="!loading && emails.length > 0" hover responsive>
          <CTableHead>
            <CTableRow>
              <CTableHeaderCell>Date & Time</CTableHeaderCell>
              <CTableHeaderCell>From</CTableHeaderCell>
              <CTableHeaderCell>Actions</CTableHeaderCell>
            </CTableRow>
          </CTableHead>
          <CTableBody>
            <CTableRow v-for="email in emails" :key="email.id">
              <CTableDataCell><DateTimeFormat :value="email.dateTime" type="dateTime" defaultText="Unknown" /></CTableDataCell>
              <CTableDataCell>
                {{ email.fromName || 'Unknown' }} 
                <small class="text-muted d-block">{{ email.fromEmail }}</small>
              </CTableDataCell>
              <CTableDataCell>
                <CButton 
                  color="primary" 
                  size="sm" 
                  @click="viewEmail(email.id)"
                  class="me-2"
                >
                  <CIcon icon="cil-envelope-open" /> View Email
                </CButton>
                <CButton 
                  v-if="email.hasImage" 
                  color="info" 
                  size="sm" 
                  @click="viewImage(email.id)"
                >
                  <CIcon icon="cil-image" /> View Image
                </CButton>
              </CTableDataCell>
            </CTableRow>
          </CTableBody>
        </CTable>
      </CCardBody>
    </CCard>
    
    <!-- Email Content Modal -->
    <CModal 
      :visible="emailModalVisible" 
      @close="closeEmailModal" 
      size="xl"
      :title="'Email from ' + (selectedEmail ? (selectedEmail.fromName || 'Unknown') : '')"
      class="email-content-modal"
    >
      <CModalBody class="email-content-container">
        <div v-if="emailContentLoading" class="text-center my-3">
          <CSpinner />
          <p class="mt-2">Loading email content...</p>
        </div>
        <div v-if="emailContentError" class="text-danger">
          {{ emailContentError }}
        </div>
        <div v-if="!emailContentLoading && !emailContentError" class="email-content-wrapper">
          <EmailViewer :html="sanitizedEmailContent" />
        </div>
      </CModalBody>
      <CModalFooter>
        <CButton color="secondary" @click="closeEmailModal">Close</CButton>
      </CModalFooter>
    </CModal>
    
    <!-- Image Modal -->
    <CModal 
      :visible="imageModalVisible" 
      @close="closeImageModal" 
      size="lg"
      :title="imageModalTitle"
      class="image-modal"
    >
      <CModalBody>
        <div v-if="imageLoading" class="text-center my-3">
          <CSpinner />
          <p class="mt-2">Loading image...</p>
        </div>
        <div v-if="imageError" class="text-danger">
          {{ imageError }}
        </div>
        <div v-if="!imageLoading && !imageError" class="text-center image-container">
          <img :src="imageUrl" alt="Email attachment" class="img-fluid" />
        </div>
      </CModalBody>
      <CModalFooter>
        <CButton color="primary" @click="downloadImage" v-if="!imageLoading && !imageError">
          <CIcon icon="cil-cloud-download" /> Download
        </CButton>
        <CButton color="secondary" @click="closeImageModal">Close</CButton>
      </CModalFooter>
    </CModal>
  </div>
</template>

<script>
import DateTimeFormat from '@/components/DateTimeFormat.vue'
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from '@/composables/toast'
import { useAuthStore } from '@/stores/auth.store'
import { cameraService } from '@/services/camera.service'
import { emailArchiveService } from '@/services/emailarchive.service'
import DOMPurify from 'dompurify'
import EmailViewer from '@/components/EmailViewer.vue'

export default {
  name: 'EmailArchive',
  components: {
    DateTimeFormat,
    EmailViewer
  },
  
  setup() {
    const route = useRoute()
    const router = useRouter()
    const { addToast } = useToast()
    
    // Camera information
    const cameraId = ref(route.params.id)
    const cameraName = ref('')
    
    // Email list state
    const emails = ref([])
    const loading = ref(true)
    const error = ref(null)
    
    // Email content modal state
    const emailModalVisible = ref(false)
    const selectedEmail = ref(null)
    const emailContent = ref('')
    const sanitizedEmailContent = computed(() => {
      return DOMPurify.sanitize(emailContent.value, {
        ADD_TAGS: ['style'],
        FORBID_TAGS: ['script', 'iframe', 'object', 'embed'],
        FORBID_ATTR: ['onerror', 'onload', 'onclick', 'onmouseover', 'onmouseout']
      })
    })
    const emailContentLoading = ref(false)
    const emailContentError = ref(null)
    
    // Image modal state
    const imageModalVisible = ref(false)
    const imageUrl = ref('')
    const imageLoading = ref(false)
    const imageError = ref(null)
    
    // Computed property for image modal title to avoid null reference errors
    const imageModalTitle = computed(() => {
      if (!selectedEmail.value) return 'Image'
      return 'Image from ' + (selectedEmail.value.fromName || 'Unknown')
    })
    
    // Fetch camera details and email archives
    const fetchData = async () => {
      loading.value = true
      error.value = null
      
      try {
        // Get camera details
        const camera = await cameraService.getById(cameraId.value)
        cameraName.value = camera.name
        
        // Get email archives
        emails.value = await emailArchiveService.getAll(cameraId.value)
      } catch (err) {
        error.value = 'Failed to load email archives. Please try again later.'
        addToast({
          title: 'Error',
          message: 'Failed to load email archives',
          color: 'danger'
        })
      } finally {
        loading.value = false
      }
    }
    
    // View email content
    const viewEmail = async (id) => {
      emailModalVisible.value = true
      emailContentLoading.value = true
      emailContentError.value = null
      emailContent.value = ''
      
      // Find the selected email in the list
      selectedEmail.value = emails.value.find(email => email.id === id)
      
      try {
        emailContent.value = await emailArchiveService.getContent(cameraId.value, id)
      } catch (err) {
        emailContentError.value = 'Failed to load email content. Please try again later.'
      } finally {
        emailContentLoading.value = false
      }
    }
    
    // View image attachment
    const viewImage = async (id) => {
      imageModalVisible.value = true
      imageLoading.value = true
      imageError.value = null
      imageUrl.value = ''
      
      // Find the selected email in the list for the modal title
      selectedEmail.value = emails.value.find(email => email.id === id)
      
      try {
        // Fetch the image with authentication and get a blob URL
        imageUrl.value = await emailArchiveService.fetchImageWithAuth(cameraId.value, id)
        imageLoading.value = false
      } catch (err) {
        console.error('Failed to load image:', err)
        imageError.value = 'Failed to load image. Please try again later.'
        imageLoading.value = false
      }
    }
    
    // Download the current image
    const downloadImage = async () => {
      if (!selectedEmail.value) return
      
      try {
        // Get the auth token and fetch the image as a blob
        const authStore = useAuthStore()
        const token = await authStore.getToken()
        
        if (!token) {
          toast.error('Authentication required to download image')
          return
        }
        
        const response = await fetch(
          `${import.meta.env.VITE_API_URL || 'http://localhost:5000/api'}/cameras/${cameraId.value}/EmailArchive/${selectedEmail.value.id}/Image`,
          { headers: { Authorization: `Bearer ${token}` } }
        )
        
        if (!response.ok) {
          throw new Error(`Failed to download image: ${response.status} ${response.statusText}`)
        }
        
        const blob = await response.blob()
        const url = window.URL.createObjectURL(blob)
        
        // Create a temporary link element
        const link = document.createElement('a')
        link.href = url
        link.download = `image-${selectedEmail.value?.id || 'attachment'}.jpg`
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
      } catch (error) {
        console.error('Error downloading image:', error)
        toast.error('Failed to download image. Please try again.')
      }
    }
    
    // Close modals
    const closeEmailModal = () => {
      emailModalVisible.value = false
      selectedEmail.value = null
      emailContent.value = ''
    }
    
    const closeImageModal = () => {
      imageModalVisible.value = false
      imageUrl.value = ''
    }
    
    // Using the centralized date formatting system instead of local formatDateTime function
    
    // Navigate back to cameras listing
    const goBack = () => {
      router.push('/cameras')
    }
    
    // Refresh email list
    const refreshEmails = async () => {
      loading.value = true
      error.value = null
      
      try {
        emails.value = await emailArchiveService.getAll(cameraId.value)
        addToast({
          title: 'Success',
          message: 'Email list refreshed',
          color: 'success'
        })
      } catch (err) {
        error.value = 'Failed to refresh email archives. Please try again later.'
        addToast({
          title: 'Error',
          message: 'Failed to refresh email archives',
          color: 'danger'
        })
      } finally {
        loading.value = false
      }
    }
    
    // Load data on component mount
    onMounted(() => {
      fetchData()
    })
    
    return {
      cameraId,
      cameraName,
      emails,
      loading,
      error,
      emailModalVisible,
      selectedEmail,
      emailContent,
      sanitizedEmailContent,
      emailContentLoading,
      emailContentError,
      imageModalVisible,
      imageUrl,
      imageLoading,
      imageError,
      imageModalTitle,
      viewEmail,
      viewImage,
      downloadImage,
      closeEmailModal,
      closeImageModal,
      // formatDateTime removed in favor of DateTimeFormat component
      goBack,
      refreshEmails
    }
  }
}
</script>

<style scoped>
.email-content-modal :deep(.modal-dialog) {
  max-width: 90%;
  height: 90%;
}

.email-content-container {
  max-height: 70vh;
  overflow: hidden;
}

.email-content-wrapper {
  height: 100%;
  overflow: hidden;
}

.image-modal :deep(.modal-dialog) {
  max-width: 80%;
}

.image-container {
  padding: 10px;
  background-color: #f8f9fa;
  border-radius: 4px;
}

.image-container img {
  max-height: 60vh;
  object-fit: contain;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}
</style>
