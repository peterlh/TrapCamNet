<template>
  <CRow>
    <CCol :md="12">
      <CCard class="mb-4">
        <CCardHeader>
          <strong>Device Notifications</strong>
          <div class="float-end">
            <CButton color="primary" size="sm" @click="showAddDeviceModal = true">
              <CIcon icon="cil-plus" /> Register New Device
            </CButton>
          </div>
        </CCardHeader>
        <CCardBody>
          <CRow class="mb-3">
            <CCol :md="6">
              <CInputGroup>
                <CFormInput
                  placeholder="Search devices..."
                  v-model="searchQuery"
                  @keyup.enter="searchDevices"
                />
                <CButton color="primary" @click="searchDevices">
                  <CIcon icon="cil-magnifying-glass" />
                </CButton>
              </CInputGroup>
            </CCol>
          </CRow>

          <CAlert v-if="error" color="danger" dismissible>
            {{ error }}
          </CAlert>

          <CAlert v-if="success" color="success" dismissible>
            {{ success }}
          </CAlert>

          <!-- Device List -->
          <CTable hover responsive>
            <CTableHead>
              <CTableRow>
                <CTableHeaderCell scope="col">Device Name</CTableHeaderCell>
                <CTableHeaderCell scope="col">Subscribed Cameras</CTableHeaderCell>
                <CTableHeaderCell scope="col">Actions</CTableHeaderCell>
              </CTableRow>
            </CTableHead>
            <CTableBody>
              <CTableRow v-for="device in devices" :key="device.id">
                <CTableDataCell>{{ device.name }}</CTableDataCell>
                <CTableDataCell>
                  <CBadge v-for="subscription in device.subscriptions" 
                    :key="subscription.id" 
                    color="info"
                    class="me-1 mb-1">
                    {{ subscription.name }}
                  </CBadge>
                  <span v-if="device.subscriptions.length === 0" class="text-muted">
                    No camera subscriptions
                  </span>
                </CTableDataCell>
                <CTableDataCell>
                  <CButton color="info" size="sm" class="me-1" @click="editDevice(device)">
                    <CIcon icon="cil-pencil" />
                  </CButton>
                  <CButton color="danger" size="sm" @click="confirmDeleteDevice(device)">
                    <CIcon icon="cil-trash" />
                  </CButton>
                </CTableDataCell>
              </CTableRow>
              <CTableRow v-if="devices.length === 0">
                <CTableDataCell colspan="3" class="text-center">
                  No devices registered yet.
                </CTableDataCell>
              </CTableRow>
            </CTableBody>
          </CTable>
      </CCardBody>
    </CCard>
    </CCol>

    <!-- Add Device Modal -->
    <CModal
      :visible="showAddDeviceModal"
      @close="() => { showAddDeviceModal = false }"
      title="Register New Device"
    >
      <CModalHeader>
        <CModalTitle>Register New Device</CModalTitle>
      </CModalHeader>
      <CModalBody>
        <CForm>
          <div class="mb-3">
            <CFormLabel for="deviceName">Device Name</CFormLabel>
            <CFormInput
              id="deviceName"
              v-model="newDevice.name"
              placeholder="Enter a name for this device"
              :disabled="isLoading"
            />
          </div>

          <div class="mb-3">
            <CFormLabel for="cameraSubscriptions">Camera Subscriptions</CFormLabel>
            <CFormSelect
              id="cameraSubscriptions"
              v-model="newDevice.cameraIds"
              :options="cameraOptions"
              multiple
              :disabled="isLoading || !cameras.length"
            />
            <CFormText v-if="!cameras.length">
              You don't have any cameras yet. Add cameras first to subscribe to notifications.
            </CFormText>
          </div>
          
          <div class="mb-3">
            <CFormCheck
              id="notifyOnlyOnAnimalDetection"
              v-model="newDevice.notifyOnlyOnAnimalDetection"
              label="Only notify when animals are detected"
              :disabled="isLoading"
            />
            <CFormText>
              When enabled, you will only receive notifications for images that contain detected animals.
            </CFormText>
          </div>

          <CAlert v-if="!fcmToken && !isRequestingPermission" color="warning">
            You need to allow notifications to register this device.
            <CButton color="primary" size="sm" class="ms-2" @click="requestPermission">
              Allow Notifications
            </CButton>
          </CAlert>

          <CAlert v-if="isRequestingPermission" color="info">
            Requesting notification permission...
          </CAlert>
        </CForm>
      </CModalBody>
      <CModalFooter>
        <CButton color="secondary" @click="showAddDeviceModal = false">
          Cancel
        </CButton>
        <CButton 
          color="primary" 
          @click="registerDevice" 
          :disabled="isLoading || !fcmToken || !newDevice.name">
          <CSpinner v-if="isLoading" size="sm" class="me-1" />
          Register
        </CButton>
      </CModalFooter>
    </CModal>

    <!-- Edit Device Modal -->
    <CModal
      :visible="showEditDeviceModal"
      @close="() => { showEditDeviceModal = false }"
      title="Edit Device Subscriptions"
    >
      <CModalHeader>
        <CModalTitle>Edit Device: {{ editingDevice?.name }}</CModalTitle>
      </CModalHeader>
      <CModalBody v-if="editingDevice">
        <CForm>
          <div class="mb-3">
            <CFormLabel for="editCameraSubscriptions">Camera Subscriptions</CFormLabel>
            <CFormSelect
              id="editCameraSubscriptions"
              v-model="editingDevice.cameraIds"
              :options="cameraOptions"
              multiple
              :disabled="isLoading || !cameras.length"
            />
          </div>
          
          <div class="mb-3">
            <CFormCheck
              id="editNotifyOnlyOnAnimalDetection"
              v-model="editingDevice.notifyOnlyOnAnimalDetection"
              label="Only notify when animals are detected"
              :disabled="isLoading"
            />
            <CFormText>
              When enabled, you will only receive notifications for images that contain detected animals.
            </CFormText>
          </div>
        </CForm>
      </CModalBody>
      <CModalFooter>
        <CButton color="secondary" @click="showEditDeviceModal = false">
          Cancel
        </CButton>
        <CButton color="primary" @click="updateDeviceSubscriptions" :disabled="isLoading">
          <CSpinner v-if="isLoading" size="sm" class="me-1" />
          Save Changes
        </CButton>
      </CModalFooter>
    </CModal>

    <!-- Delete Confirmation Modal -->
    <CModal
      :visible="showDeleteModal"
      @close="() => { showDeleteModal = false }"
      title="Confirm Delete"
    >
      <CModalHeader>
        <CModalTitle>Confirm Delete</CModalTitle>
      </CModalHeader>
      <CModalBody>
        Are you sure you want to delete device "{{ deletingDevice?.name }}"? This action cannot be undone.
      </CModalBody>
      <CModalFooter>
        <CButton color="secondary" @click="showDeleteModal = false">
          Cancel
        </CButton>
        <CButton color="danger" @click="deleteDevice" :disabled="isLoading">
          <CSpinner v-if="isLoading" size="sm" class="me-1" />
          Delete
        </CButton>
      </CModalFooter>
    </CModal>
  </CRow>
</template>

<script>
import { ref, onMounted, computed, inject } from 'vue'
import { notificationService } from '@/services/notification.service'
import { cameraService } from '@/services/camera.service'
import { requestNotificationPermission } from '@/firebase/messaging'
import { useToast } from '@/composables/toast'

export default {
  name: 'Notifications',
  setup() {
    const devices = ref([])
    const cameras = ref([])
    const error = ref('')
    const success = ref('')
    const isLoading = ref(false)
    const fcmToken = ref('')
    const isRequestingPermission = ref(false)
    const searchQuery = ref('')
    const { addToast } = useToast()
    const icons = inject('icons')
    
    // Modals
    const showAddDeviceModal = ref(false)
    const showEditDeviceModal = ref(false)
    const showDeleteModal = ref(false)
    
    // Form data
    const newDevice = ref({
      name: '',
      cameraIds: [],
      notifyOnlyOnAnimalDetection: false
    })
    
    const editingDevice = ref(null)
    const deletingDevice = ref(null)
    
    // Computed properties
    const cameraOptions = computed(() => {
      console.log('Computing camera options from cameras:', cameras.value)
      const options = cameras.value.map(camera => ({
        value: camera.id,
        text: camera.name
      }))
      console.log('Generated camera options:', options)
      return options
    })
    
    // Methods
    const loadDevices = async (search = '') => {
      try {
        isLoading.value = true
        // Note: If the notificationService doesn't support search yet, we'll filter client-side
        const allDevices = await notificationService.getDevices()
        
        if (search && search.trim() !== '') {
          // Client-side filtering if the API doesn't support search
          const searchLower = search.toLowerCase()
          devices.value = allDevices.filter(device => 
            device.name.toLowerCase().includes(searchLower) ||
            device.subscriptions.some(sub => sub.name.toLowerCase().includes(searchLower))
          )
        } else {
          devices.value = allDevices
        }
      } catch (err) {
        error.value = 'Failed to load devices: ' + (err.response?.data?.message || err.message)
        addToast({
          title: 'Error',
          message: 'Failed to load notification devices',
          color: 'danger'
        })
      } finally {
        isLoading.value = false
      }
    }
    
    const searchDevices = () => {
      loadDevices(searchQuery.value)
    }
    
    const loadCameras = async () => {
      try {
        isLoading.value = true
        console.log('Loading cameras...')
        cameras.value = await cameraService.getAll()
        console.log('Cameras loaded:', cameras.value)
      } catch (err) {
        console.error('Error loading cameras:', err)
        error.value = 'Failed to load cameras: ' + (err.response?.data?.message || err.message)
      } finally {
        isLoading.value = false
      }
    }
    
    const requestPermission = async () => {
      isRequestingPermission.value = true
      try {
        const token = await requestNotificationPermission()
        if (token) {
          fcmToken.value = token
          success.value = 'Notification permission granted!'
        } else {
          error.value = 'Notification permission denied. You need to allow notifications to receive alerts.'
        }
      } catch (err) {
        error.value = 'Error requesting notification permission: ' + err.message
      } finally {
        isRequestingPermission.value = false
      }
    }
    
    const registerDevice = async () => {
      if (!fcmToken.value) {
        error.value = 'Notification permission is required'
        return
      }
      
      if (!newDevice.value.name) {
        error.value = 'Device name is required'
        return
      }
      
      try {
        isLoading.value = true
        error.value = ''
        success.value = ''
        
        await notificationService.registerDevice({
          name: newDevice.value.name,
          fcmToken: fcmToken.value,
          cameraIds: newDevice.value.cameraIds,
          notifyOnlyOnAnimalDetection: newDevice.value.notifyOnlyOnAnimalDetection
        })
        
        // Reset form and close modal
        newDevice.value = { name: '', cameraIds: [] }
        showAddDeviceModal.value = false
        
        // Reload devices
        await loadDevices()
        
        success.value = 'Device registered successfully!'
      } catch (err) {
        error.value = 'Failed to register device: ' + (err.response?.data?.message || err.message)
      } finally {
        isLoading.value = false
      }
    }
    
    const editDevice = (device) => {
      editingDevice.value = {
        id: device.id,
        name: device.name,
        cameraIds: device.subscriptions.map(s => s.id),
        notifyOnlyOnAnimalDetection: device.notifyOnlyOnAnimalDetection || false
      }
      showEditDeviceModal.value = true
    }
    
    const updateDeviceSubscriptions = async () => {
      if (!editingDevice.value) return
      
      try {
        isLoading.value = true
        error.value = ''
        success.value = ''
        
        await notificationService.updateDeviceSubscriptions(
          editingDevice.value.id,
          editingDevice.value.cameraIds,
          editingDevice.value.notifyOnlyOnAnimalDetection
        )
        
        // Close modal and reload devices
        showEditDeviceModal.value = false
        await loadDevices()
        
        success.value = 'Device subscriptions updated successfully!'
      } catch (err) {
        error.value = 'Failed to update device: ' + (err.response?.data?.message || err.message)
      } finally {
        isLoading.value = false
      }
    }
    
    const confirmDeleteDevice = (device) => {
      deletingDevice.value = device
      showDeleteModal.value = true
    }
    
    const deleteDevice = async () => {
      if (!deletingDevice.value) return
      
      try {
        isLoading.value = true
        error.value = ''
        success.value = ''
        
        await notificationService.deleteDevice(deletingDevice.value.id)
        
        // Close modal and reload devices
        showDeleteModal.value = false
        await loadDevices()
        
        success.value = 'Device deleted successfully!'
      } catch (err) {
        error.value = 'Failed to delete device: ' + (err.response?.data?.message || err.message)
      } finally {
        isLoading.value = false
      }
    }
    
    // Lifecycle hooks
    onMounted(async () => {
      await Promise.all([loadDevices(), loadCameras()])
      
      // Check if we already have notification permission
      if (Notification.permission === 'granted') {
        try {
          const token = await requestNotificationPermission()
          if (token) {
            fcmToken.value = token
          }
        } catch (err) {
          console.error('Error getting FCM token:', err)
        }
      }
    })
    
    return {
      devices,
      cameras,
      error,
      success,
      isLoading,
      fcmToken,
      isRequestingPermission,
      showAddDeviceModal,
      showEditDeviceModal,
      showDeleteModal,
      newDevice,
      editingDevice,
      deletingDevice,
      cameraOptions,
      searchQuery,
      searchDevices,
      requestPermission,
      registerDevice,
      editDevice,
      updateDeviceSubscriptions,
      confirmDeleteDevice,
      deleteDevice
    }
  }
}
</script>
