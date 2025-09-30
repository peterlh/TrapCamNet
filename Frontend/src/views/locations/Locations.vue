<template>
  <CRow>
    <CCol :md="12">
      <CCard class="mb-4">
        <CCardHeader>
          <strong>Locations</strong>
          <div class="float-end">
            <CButton color="primary" size="sm" @click="openAddModal">
              <CIcon icon="cil-plus" /> Add Location
            </CButton>
          </div>
        </CCardHeader>
        <CCardBody>
          <CRow class="mb-3">
            <CCol :md="6">
              <CInputGroup>
                <CFormInput
                  placeholder="Search locations..."
                  v-model="searchQuery"
                  @keyup.enter="searchLocations"
                />
                <CButton color="primary" @click="searchLocations">
                  <CIcon icon="cil-magnifying-glass" />
                </CButton>
              </CInputGroup>
            </CCol>
          </CRow>
          
          <CTable striped hover responsive>
            <CTableHead>
              <CTableRow>
                <CTableHeaderCell scope="col">Name</CTableHeaderCell>
                <CTableHeaderCell scope="col">Note</CTableHeaderCell>
                <CTableHeaderCell scope="col">Coordinates</CTableHeaderCell>
                <CTableHeaderCell scope="col">Actions</CTableHeaderCell>
              </CTableRow>
            </CTableHead>
            <CTableBody>
              <CTableRow v-for="location in locations" :key="location.id">
                <CTableDataCell>{{ location.name }}</CTableDataCell>
                <CTableDataCell>{{ location.note || 'N/A' }}</CTableDataCell>
                <CTableDataCell>{{ formatCoordinates(location.latitude, location.longitude) }}</CTableDataCell>
                <CTableDataCell>
                  <CButton color="info" size="sm" class="me-1" @click="openEditModal(location)">
                    <CIcon icon="cil-pencil" />
                  </CButton>
                  <CButton color="danger" size="sm" @click="confirmDelete(location)">
                    <CIcon icon="cil-trash" />
                  </CButton>
                </CTableDataCell>
              </CTableRow>
              <CTableRow v-if="locations.length === 0">
                <CTableDataCell colspan="4" class="text-center">
                  No locations found
                </CTableDataCell>
              </CTableRow>
            </CTableBody>
          </CTable>
        </CCardBody>
      </CCard>
    </CCol>
  </CRow>

  <!-- Add/Edit Location Modal -->
  <CModal
    :visible="modalVisible"
    @close="() => { modalVisible = false }"
    :title="isEditing ? 'Edit Location' : 'Add Location'"
    size="lg"
  >
    <CModalHeader>
      <CModalTitle>{{ isEditing ? 'Edit Location' : 'Add Location' }}</CModalTitle>
    </CModalHeader>
    <CModalBody>
      <CForm>
        <CRow>
          <CCol :md="12" class="mb-3">
            <CFormLabel for="locationName">Name</CFormLabel>
            <CFormInput
              id="locationName"
              v-model.trim="locationForm.name"
              placeholder="Enter location name"
              :invalid="!!validationErrors.name"
              @input="validationErrors.name = ''"
            />
            <CFormFeedback invalid v-if="validationErrors.name">
              {{ validationErrors.name }}
            </CFormFeedback>
          </CCol>
        </CRow>
        
        <CRow>
          <CCol :md="12" class="mb-3">
            <CFormLabel for="locationNote">Note</CFormLabel>
            <CFormTextarea
              id="locationNote"
              v-model.trim="locationForm.note"
              placeholder="Enter notes about this location"
              rows="3"
            />
          </CCol>
        </CRow>
        
        <CRow>
          <CCol :md="12" class="mb-3">
            <CFormLabel>Location</CFormLabel>
            <div class="map-container" style="height: 300px; width: 100%;">
              <LMap
                ref="map"
                v-model:zoom="zoom"
                :center="mapCenter"
                @click="updateLocationFromMap"
                style="height: 100%"
              >
                <LTileLayer
                  v-for="(tileProvider, name) in tileLayers"
                  :key="name"
                  :visible="currentTileLayer === name"
                  :url="tileProvider.url"
                  :attribution="tileProvider.attribution"
                  layer-type="base"
                  :name="name"
                />
                <LControl position="topright">
                  <div class="leaflet-bar leaflet-control bg-white p-1">
                    <CDropdown variant="btn-group" :popper="false">
                      <CDropdownToggle color="light">{{ currentTileLayer }}</CDropdownToggle>
                      <CDropdownMenu>
                        <CDropdownItem v-for="(_, name) in tileLayers" :key="name" @click="currentTileLayer = name">
                          {{ name }}
                        </CDropdownItem>
                      </CDropdownMenu>
                    </CDropdown>
                  </div>
                </LControl>
                <LMarker
                  v-if="hasCoordinates"
                  :lat-lng="markerPosition"
                  draggable
                  ref="marker"
                  @dragend="updateLocationFromMarker"
                />
              </LMap>
            </div>
            <small class="text-muted mt-1 d-block">
              Click on the map to set location or drag the marker to adjust. Coordinates can only be modified by drag and drop.
            </small>
            <div class="d-flex mt-2">
              <div class="coordinates-display p-2 border rounded me-2 flex-grow-1">
                Latitude: {{ locationForm.latitude !== null ? locationForm.latitude : 'Not set' }}
              </div>
              <div class="coordinates-display p-2 border rounded flex-grow-1">
                Longitude: {{ locationForm.longitude !== null ? locationForm.longitude : 'Not set' }}
              </div>
            </div>
            <CFormFeedback invalid v-if="validationErrors.coordinates">
              {{ validationErrors.coordinates }}
            </CFormFeedback>
          </CCol>
        </CRow>
      </CForm>
    </CModalBody>
    <CModalFooter>
      <CButton color="secondary" @click="modalVisible = false">
        Cancel
      </CButton>
      <CButton color="primary" @click="saveLocation">
        {{ isEditing ? 'Update' : 'Save' }}
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
      Are you sure you want to delete the location "{{ selectedLocation?.name }}"?
      This action cannot be undone.
    </CModalBody>
    <CModalFooter>
      <CButton color="secondary" @click="deleteModalVisible = false">
        Cancel
      </CButton>
      <CButton color="danger" @click="deleteLocation">
        Delete
      </CButton>
    </CModalFooter>
  </CModal>
</template>

<script>
import { ref, onMounted, reactive, computed } from 'vue'
import { locationService } from '@/services/location.service'
import { useToast } from '@/composables/toast'
import { LMap, LTileLayer, LMarker, LControl } from '@vue-leaflet/vue-leaflet'
import L from 'leaflet'

export default {
  name: 'Locations',
  components: {
    LMap,
    LTileLayer,
    LMarker,
    LControl
  },
  setup() {
    const { addToast } = useToast()
    
    // Fix Leaflet icon paths
    const fixLeafletIcon = () => {
      // This is needed to make Leaflet markers work with Vite
      delete L.Icon.Default.prototype._getIconUrl
      L.Icon.Default.mergeOptions({
        iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
        iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
        shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
      })
    }
    
    // Data
    const locations = ref([])
    const searchQuery = ref('')
    const modalVisible = ref(false)
    const deleteModalVisible = ref(false)
    const isEditing = ref(false)
    const selectedLocation = ref(null)
    const validationErrors = reactive({
      name: '',
      coordinates: '',
      latitude: '',
      longitude: ''
    })
    
    const map = ref(null)
    
    const locationForm = reactive({
      name: '',
      note: '',
      latitude: null,
      longitude: null
    })
    
    // Map related data
    const zoom = ref(10)
    const mapCenter = ref([0, 0])
    const marker = ref(null)
    
    // Tile layer options
    const currentTileLayer = ref('Satellite')
    const tileLayers = {
      'OpenStreetMap': {
        url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
      },
      'Satellite': {
        url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
        attribution: 'Tiles &copy; Esri &mdash; Source: Esri, i-cubed, USDA, USGS, AEX, GeoEye, Getmapping, Aerogrid, IGN, IGP, UPR-EGP, and the GIS User Community'
      },
      'Terrain': {
        url: 'https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png',
        attribution: 'Map data: &copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, <a href="http://viewfinderpanoramas.org">SRTM</a> | Map style: &copy; <a href="https://opentopomap.org">OpenTopoMap</a>'
      }
    }
    
    const markerPosition = computed(() => {
      return [Number(locationForm.latitude) || 0, Number(locationForm.longitude) || 0]
    })
    
    const hasCoordinates = computed(() => {
      return locationForm.latitude !== null && locationForm.longitude !== null
    })
    
    // Methods
    const fetchLocations = async (search = '') => {
      try {
        const data = await locationService.getAll(search)
        locations.value = data
      } catch (error) {
        console.error('Error fetching locations:', error)
        addToast({
          title: 'Error',
          message: 'Failed to load locations',
          color: 'danger'
        })
      }
    }
    
    const searchLocations = () => {
      fetchLocations(searchQuery.value)
    }
    
    const openAddModal = () => {
      isEditing.value = false
      selectedLocation.value = null
      resetForm()
      
      // Set default map center (can be customized to a default location)
      if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
          (position) => {
            mapCenter.value = [position.coords.latitude, position.coords.longitude]
          },
          () => {
            // Default to a central location if geolocation fails
            mapCenter.value = [0, 0]
          }
        )
      }
      
      modalVisible.value = true
    }
    
    const openEditModal = (location) => {
      isEditing.value = true
      selectedLocation.value = location
      
      locationForm.name = location.name
      locationForm.note = location.note || ''
      locationForm.latitude = location.latitude
      locationForm.longitude = location.longitude
      
      mapCenter.value = [location.latitude, location.longitude]
      
      modalVisible.value = true
    }
    
    const resetForm = () => {
      locationForm.name = ''
      locationForm.note = ''
      locationForm.latitude = null
      locationForm.longitude = null
      
      Object.keys(validationErrors).forEach(key => {
        validationErrors[key] = ''
      })
    }
    
    const validateForm = () => {
      let isValid = true
      
      console.log('Validating location form:', locationForm)
      
      // Validate name
      if (!locationForm.name || !locationForm.name.trim()) {
        validationErrors.name = 'Location name is required'
        isValid = false
        console.log('Validation failed: Location name is empty')
      } else {
        validationErrors.name = ''
      }
      
      // Validate coordinates
      if (locationForm.latitude === null || locationForm.longitude === null) {
        validationErrors.coordinates = 'Please select a location on the map'
        isValid = false
        console.log('Validation failed: Coordinates not selected')
      } else {
        validationErrors.coordinates = ''
      }
      
      return isValid
    }
    
    const saveLocation = async () => {
      console.log('Saving location with data:', locationForm)
      
      if (!validateForm()) {
        console.log('Form validation failed')
        return
      }
      
      try {
        const locationData = {
          name: locationForm.name.trim(),
          note: locationForm.note.trim(),
          latitude: parseFloat(locationForm.latitude),
          longitude: parseFloat(locationForm.longitude)
        }
        
        console.log('Sending location data to API:', locationData)
        
        if (isEditing.value) {
          await locationService.update(selectedLocation.value.id, locationData)
          
          addToast({
            title: 'Success',
            message: 'Location updated successfully',
            color: 'success'
          })
        } else {
          await locationService.create(locationData)
          
          addToast({
            title: 'Success',
            message: 'Location created successfully',
            color: 'success'
          })
        }
        
        modalVisible.value = false
        fetchLocations()
      } catch (error) {
        console.error('Error saving location:', error)
        
        // More detailed error handling
        let errorMessage = 'Failed to save location'
        
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
    
    const confirmDelete = (location) => {
      selectedLocation.value = location
      deleteModalVisible.value = true
    }
    
    const deleteLocation = async () => {
      if (!selectedLocation.value) return
      
      try {
        await locationService.delete(selectedLocation.value.id)
        
        addToast({
          title: 'Success',
          message: 'Location deleted successfully',
          color: 'success'
        })
        
        deleteModalVisible.value = false
        fetchLocations()
      } catch (error) {
        console.error('Error deleting location:', error)
        
        let errorMessage = 'Failed to delete location'
        
        if (error.response && error.response.data) {
          if (typeof error.response.data === 'string') {
            errorMessage = error.response.data
          }
        }
        
        addToast({
          title: 'Error',
          message: errorMessage,
          color: 'danger'
        })
      }
    }
    
    // Map related methods
    const updateLocationFromMap = (event) => {
      locationForm.latitude = parseFloat(event.latlng.lat.toFixed(6))
      locationForm.longitude = parseFloat(event.latlng.lng.toFixed(6))
      validationErrors.coordinates = ''
      
      // If no marker exists yet, it will be created by the v-if="hasCoordinates"
      // If marker already exists, update its position
      if (marker.value) {
        marker.value.setLatLng(event.latlng)
      }
    }
    
    const updateLocationFromMarker = (event) => {
      const latLng = event.target.getLatLng()
      locationForm.latitude = parseFloat(latLng.lat.toFixed(6))
      locationForm.longitude = parseFloat(latLng.lng.toFixed(6))
      validationErrors.coordinates = ''
    }
    
    // Removed updateMarkerFromInput as coordinates can only be modified via drag-drop
    
    const formatCoordinates = (lat, lng) => {
      if (lat === null || lng === null) return 'N/A'
      return `${lat}, ${lng}`
    }
    
    // Lifecycle hooks
    onMounted(() => {
      fetchLocations()
      fixLeafletIcon()
    })
    
    return {
      locations,
      searchQuery,
      modalVisible,
      deleteModalVisible,
      isEditing,
      selectedLocation,
      locationForm,
      validationErrors,
      mapCenter,
      markerPosition,
      hasCoordinates,
      map,
      zoom,
      marker,
      tileLayers,
      currentTileLayer,
      
      fetchLocations,
      searchLocations,
      openAddModal,
      openEditModal,
      saveLocation,
      confirmDelete,
      deleteLocation,
      updateLocationFromMap,
      updateLocationFromMarker,
      // updateMarkerFromInput removed,
      formatCoordinates
    }
  }
}
</script>

<style scoped>
.map-container {
  border: 1px solid #ccc;
  border-radius: 4px;
  overflow: hidden;
}

.coordinates-display {
  background-color: #f8f9fa;
  min-height: 38px;
  display: flex;
  align-items: center;
}
</style>
