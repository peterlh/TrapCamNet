<template>
  <CRow>
    <CCol :md="12">
      <CCard class="mb-4">
        <CCardHeader>
          <strong>Cameras</strong>
          <div class="float-end">
            <CButton color="primary" size="sm" @click="openAddModal">
              <CIcon icon="cil-plus" /> Add Camera
            </CButton>
          </div>
        </CCardHeader>
        <CCardBody>
          <CRow class="mb-3">
            <CCol :md="6">
              <CInputGroup>
                <CFormInput
                  placeholder="Search cameras..."
                  v-model="searchQuery"
                  @keyup.enter="searchCameras"
                />
                <CButton color="primary" @click="searchCameras">
                  <CIcon icon="cil-magnifying-glass" />
                </CButton>
              </CInputGroup>
            </CCol>
          </CRow>
          
          <CTable striped hover responsive>
            <CTableHead>
              <CTableRow>
                <CTableHeaderCell scope="col">Name</CTableHeaderCell>
                <CTableHeaderCell scope="col">Email Address</CTableHeaderCell>
                <CTableHeaderCell scope="col">Battery</CTableHeaderCell>
                <CTableHeaderCell scope="col">Location</CTableHeaderCell>
                <CTableHeaderCell scope="col">Last Contact</CTableHeaderCell>
                <CTableHeaderCell scope="col">Actions</CTableHeaderCell>
              </CTableRow>
            </CTableHead>
            <CTableBody>
              <CTableRow v-for="camera in cameras" :key="camera.id">
                <CTableDataCell>{{ camera.name }}</CTableDataCell>
                <CTableDataCell>{{ camera.inboundEmailAddress }}</CTableDataCell>
                <CTableDataCell>
                  <div v-if="camera.batteryInfo?.voltage">
                    <CBadge :color="getBatteryVoltageColor(camera.batteryInfo.voltage)">
                      {{ camera.batteryInfo.voltage }}V
                    </CBadge>
                  </div>
                  <div v-else>
                    <CBadge :color="getBatteryColor(camera.lastBatteryState)">
                      {{ camera.lastBatteryState }}%
                    </CBadge>
                  </div>
                </CTableDataCell>
                <CTableDataCell>{{ camera.currentLocation?.name || 'N/A' }}</CTableDataCell>
                <CTableDataCell>
                  <span v-if="camera.lastContact">
                    <DateTimeFormat :value="camera.lastContact" type="dateTime" defaultText="Never" />
                  </span>
                  <span v-else class="text-muted">Never</span>
                </CTableDataCell>
                <CTableDataCell>
                  <CButton color="info" size="sm" class="me-1" @click="openEditModal(camera)">
                    <CIcon icon="cil-pencil" />
                  </CButton>
                  <CButton color="success" size="sm" class="me-1" @click="viewEmailArchive(camera)">
                    <CIcon icon="cil-envelope-open" />
                  </CButton>
                  <CButton color="danger" size="sm" @click="confirmDelete(camera)">
                    <CIcon icon="cil-trash" />
                  </CButton>
                </CTableDataCell>
              </CTableRow>
              <CTableRow v-if="cameras.length === 0">
                <CTableDataCell colspan="6" class="text-center">
                  No cameras found
                </CTableDataCell>
              </CTableRow>
            </CTableBody>
          </CTable>
        </CCardBody>
      </CCard>
    </CCol>
  </CRow>

  <!-- Add/Edit Camera Modal -->
  <CModal
    :visible="modalVisible"
    @close="() => { modalVisible = false }"
    :title="isEditing ? 'Edit Camera' : 'Add Camera'"
  >
    <CModalHeader>
      <CModalTitle>{{ isEditing ? 'Edit Camera' : 'Add Camera' }}</CModalTitle>
    </CModalHeader>
    <CModalBody>
      <CForm>
        <CRow>
          <CCol :md="12">
            <CFormLabel for="cameraName">Name</CFormLabel>
            <CFormInput
              id="cameraName"
              v-model.trim="cameraForm.name"
              placeholder="Enter camera name"
              :invalid="!!validationErrors.name"
              @input="validationErrors.name = ''"
            />
            <CFormFeedback invalid v-if="validationErrors.name">
              {{ validationErrors.name }}
            </CFormFeedback>
          </CCol>
        </CRow>

        <CRow class="mt-3" v-if="isEditing">
          <CCol :md="12">
            <CFormLabel for="emailAddress">Email Address</CFormLabel>
            <CFormInput
              id="emailAddress"
              v-model="cameraForm.inboundEmailAddress"
              disabled
              readonly
            />
            <CFormText>Email address is auto-generated and cannot be changed</CFormText>
          </CCol>
        </CRow>

        <!-- Battery state field removed from popup form as requested -->

        <CRow class="mt-3">
          <CCol :md="12">
            <CFormLabel for="locationSelect">Location</CFormLabel>
            <CFormSelect
              id="locationSelect"
              v-model="cameraForm.locationId"
            >
              <option :value="null">No Location</option>
              <option v-for="location in locations" :key="location.id" :value="location.id">
                {{ location.name }}
              </option>
            </CFormSelect>
          </CCol>
        </CRow>

      </CForm>
    </CModalBody>
    <CModalFooter>
      <CButton color="secondary" @click="modalVisible = false">
        Cancel
      </CButton>
      <CButton color="primary" @click="saveCamera">
        {{ isEditing ? 'Update' : 'Create' }}
      </CButton>
    </CModalFooter>
  </CModal>

  <!-- Delete Confirmation Modal -->
  <CModal
    :visible="deleteModalVisible"
    @close="() => { deleteModalVisible = false }"
    title="Confirm Delete"
  >
    <CModalHeader>
      <CModalTitle>Confirm Delete</CModalTitle>
    </CModalHeader>
    <CModalBody>
      Are you sure you want to delete the camera "{{ selectedCamera?.name }}"?
      This action cannot be undone.
    </CModalBody>
    <CModalFooter>
      <CButton color="secondary" @click="deleteModalVisible = false">
        Cancel
      </CButton>
      <CButton color="danger" @click="deleteCamera">
        Delete
      </CButton>
    </CModalFooter>
  </CModal>
</template>

<script>
import DateTimeFormat from '@/components/DateTimeFormat.vue'
import { ref, onMounted, reactive, inject } from 'vue'
import { useRouter } from 'vue-router'
import { cameraService } from '@/services/camera.service'
import { locationService } from '@/services/location.service'
import { useToast } from '@/composables/toast'

export default {
  name: 'Cameras',
  components: {
    DateTimeFormat
  },
  setup() {
    const router = useRouter()
    const cameras = ref([])
    const locations = ref([])
    const modalVisible = ref(false)
    const deleteModalVisible = ref(false)
    const isEditing = ref(false)
    const selectedCamera = ref(null)
    const searchQuery = ref('')
    const { addToast } = useToast()
    const icons = inject('icons')
    
    const cameraForm = reactive({
      name: '',
      inboundEmailAddress: '',
      lastBatteryState: 100,
      locationId: null,

    })
    
    const validationErrors = reactive({
      name: ''
    })
    
    const fetchCameras = async (search = '') => {
      try {
        cameras.value = await cameraService.getAll(search)
      } catch (error) {
        console.error('Error fetching cameras:', error)
        addToast({
          title: 'Error',
          message: 'Failed to load cameras',
          color: 'danger'
        })
      }
    }
    
    const fetchLocations = async () => {
      try {
        locations.value = await locationService.getAll()
      } catch (error) {
        console.error('Error fetching locations:', error)
        addToast({
          title: 'Error',
          message: 'Failed to load locations',
          color: 'danger'
        })
      }
    }
    
    const searchCameras = () => {
      fetchCameras(searchQuery.value)
    }
    
    const resetForm = () => {
      cameraForm.name = ''
      cameraForm.inboundEmailAddress = ''
      cameraForm.lastBatteryState = 100
      cameraForm.locationId = null

      
      // Clear validation errors
      validationErrors.name = ''
    }
    
    const openAddModal = () => {
      resetForm()
      isEditing.value = false
      modalVisible.value = true
    }
    
    const openEditModal = (camera) => {
      resetForm()
      isEditing.value = true
      selectedCamera.value = camera
      
      // Populate form with camera data
      cameraForm.name = camera.name
      cameraForm.inboundEmailAddress = camera.inboundEmailAddress
      cameraForm.lastBatteryState = camera.lastBatteryState
      cameraForm.locationId = camera.currentLocation?.id || null

      
      modalVisible.value = true
    }
    
    const validateForm = () => {
      let isValid = true
      
      console.log('Validating camera name:', cameraForm.name)
      
      // Just check if the name exists and is not just whitespace
      if (!cameraForm.name || !cameraForm.name.trim()) {
        validationErrors.name = 'Camera name is required'
        isValid = false
        console.log('Validation failed: Camera name is empty')
      } else {
        validationErrors.name = ''
        console.log('Validation passed')
      }
      
      return isValid
    }
    
    const saveCamera = async () => {
      console.log('Saving camera with data:', cameraForm)
      
      if (!validateForm()) {
        console.log('Form validation failed')
        return
      }
      
      try {
        const cameraData = {
          name: cameraForm.name.trim(),
          lastBatteryState: cameraForm.lastBatteryState,
          locationId: cameraForm.locationId,

        }
        
        console.log('Sending camera data to API:', cameraData)
        
        if (isEditing.value) {
          await cameraService.update(selectedCamera.value.id, cameraData)
          
          addToast({
            title: 'Success',
            message: 'Camera updated successfully',
            color: 'success'
          })
        } else {
          await cameraService.create(cameraData)
          
          addToast({
            title: 'Success',
            message: 'Camera created successfully',
            color: 'success'
          })
        }
        
        modalVisible.value = false
        fetchCameras()
      } catch (error) {
        console.error('Error saving camera:', error)
        
        // More detailed error handling
        let errorMessage = 'Failed to save camera';
        
        if (error.response && error.response.data) {
          console.log('API error response:', error.response.data)
          if (typeof error.response.data === 'string') {
            errorMessage = error.response.data
          } else if (error.response.data.errors) {
            // Handle validation errors from API
            const errors = error.response.data.errors
            if (errors.Name) {
              validationErrors.name = errors.Name[0]
            }
            errorMessage = 'Please fix the validation errors'
          }
        }
        
        addToast({
          title: 'Error',
          message: errorMessage,
          color: 'danger'
        })
      }
    }
    
    const confirmDelete = (camera) => {
      selectedCamera.value = camera
      deleteModalVisible.value = true
    }
    
    const deleteCamera = async () => {
      try {
        await cameraService.delete(selectedCamera.value.id)
        deleteModalVisible.value = false
        
        addToast({
          title: 'Success',
          message: 'Camera deleted successfully',
          color: 'success'
        })
        
        fetchCameras()
      } catch (error) {
        console.error('Error deleting camera:', error)
        addToast({
          title: 'Error',
          message: 'Failed to delete camera',
          color: 'danger'
        })
      }
    }
    
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
    
    // Navigation to camera details page
    const viewCameraDetails = (camera) => {
      window.location.href = `#/cameras/${camera.id}`
    }
    
    // Navigation to email archive page
    const viewEmailArchive = (camera) => {
      router.push(`/cameras/${camera.id}/email-archive`)
    }
    
    onMounted(() => {
      fetchCameras()
      fetchLocations()
    })
    
    return {
      cameras,
      locations,
      modalVisible,
      deleteModalVisible,
      isEditing,
      selectedCamera,
      cameraForm,
      validationErrors,
      searchQuery,
      openAddModal,
      openEditModal,
      saveCamera,
      confirmDelete,
      deleteCamera,
      searchCameras,
      getBatteryColor,
      getBatteryVoltageColor,
      // formatDate removed in favor of DateTimeFormat component
      viewCameraDetails,
      viewEmailArchive,
      icons
    }
  }
}
</script>
