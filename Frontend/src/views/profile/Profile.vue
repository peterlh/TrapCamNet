<template>
  <div class="row">
    <div class="col-md-12">
      <!-- Toast notification -->
      <div 
        class="toast align-items-center border-0 position-fixed top-0 end-0 m-3" 
        :class="[toastType === 'success' ? 'text-bg-success' : 'text-bg-danger', { 'show': showToast }]"
        role="alert" 
        aria-live="assertive" 
        aria-atomic="true"
        style="z-index: 1050;"
      >
        <div class="d-flex">
          <div class="toast-body">
            {{ toastMessage }}
          </div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" @click="hideToast" aria-label="Close"></button>
        </div>
      </div>
      
      <!-- Profile Information -->
      <div class="card mb-4">
        <div class="card-header">
          <strong>User Profile</strong>
        </div>
        <div class="card-body">
          <form @submit.prevent="saveProfile">
            <div class="mb-3">
              <label for="displayName" class="form-label">Display Name</label>
              <input
                type="text"
                class="form-control"
                id="displayName"
                v-model="profileForm.displayName"
                placeholder="Enter your name"
                :disabled="isLoading"
              />
            </div>
            <div class="mb-3">
              <label for="email" class="form-label">Email Address</label>
              <input
                type="email"
                class="form-control"
                id="email"
                v-model="profileForm.email"
                placeholder="name@example.com"
                readonly
                disabled
              />
            </div>
            <div class="mb-3">
              <button type="submit" class="btn btn-primary" :disabled="isLoading">
                <span v-if="isLoading" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                Save Changes
              </button>
            </div>
          </form>
        </div>
      </div>
      
      <!-- Preferences -->
      <div class="card mb-4">
        <div class="card-header">
          <strong>Display Preferences</strong>
        </div>
        <div class="card-body">
          <form @submit.prevent="savePreferences">
            <!-- Date Format -->
            <div class="mb-3">
              <label for="dateFormat" class="form-label">Date Format</label>
              <select
                class="form-select"
                id="dateFormat"
                v-model="preferencesForm.dateFormat"
                :disabled="isPreferencesLoading"
              >
                <option v-for="(example, format) in preferencesStore.availableDateFormats" :key="format" :value="format">
                  {{ example }}
                </option>
              </select>
              <div class="form-text">
                Current date in selected format: {{ formatDateExample }}
              </div>
            </div>
            
            <!-- Time Format -->
            <div class="mb-3">
              <label for="timeFormat" class="form-label">Time Format</label>
              <select
                class="form-select"
                id="timeFormat"
                v-model="preferencesForm.timeFormat"
                :disabled="isPreferencesLoading"
              >
                <option v-for="(example, format) in preferencesStore.availableTimeFormats" :key="format" :value="format">
                  {{ example }}
                </option>
              </select>
              <div class="form-text">
                Current time in selected format: {{ formatTimeExample }}
              </div>
            </div>
            
            <!-- Timezone -->
            <div class="mb-3">
              <label for="timezone" class="form-label">Timezone</label>
              <select
                class="form-select"
                id="timezone"
                v-model="preferencesForm.timezone"
                :disabled="isPreferencesLoading"
              >
                <option v-for="(name, tz) in preferencesStore.availableTimezones" :key="tz" :value="tz">
                  {{ name }}
                </option>
              </select>
              <div class="form-text">
                All dates and times will be displayed in this timezone
              </div>
            </div>
            
            <!-- Preview -->
            <div class="mb-3">
              <label class="form-label">Preview</label>
              <div class="border rounded p-2 bg-light">
                <div>Date and time: {{ formatDateTimeExample }}</div>
              </div>
            </div>
            
            <!-- Buttons -->
            <div class="d-flex gap-2">
              <button type="submit" class="btn btn-primary" :disabled="isPreferencesLoading">
                <span v-if="isPreferencesLoading" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                Save Preferences
              </button>
              <button type="button" class="btn btn-outline-secondary" @click="resetPreferences" :disabled="isPreferencesLoading">
                Reset to Defaults
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { useAuthStore } from '@/stores/auth.store'
import { usePreferencesStore } from '@/stores/preferences.store'
import { auth } from '@/firebase/config'
import { updateProfile } from 'firebase/auth'
import { format } from 'date-fns'
import { toZonedTime } from 'date-fns-tz'

const authStore = useAuthStore()
const preferencesStore = usePreferencesStore()
const isLoading = ref(false)
const isPreferencesLoading = ref(false)
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success')

// Current date/time for preview
const now = new Date()

// Form data
const profileForm = reactive({
  displayName: authStore.user?.displayName || '',
  email: authStore.user?.email || ''
})

const preferencesForm = reactive({
  dateFormat: preferencesStore.dateFormat,
  timeFormat: preferencesStore.timeFormat,
  timezone: preferencesStore.timezone
})

// Computed properties for format previews
const formatDateExample = computed(() => {
  try {
    // Use date-fns-tz to handle timezone
    const zonedDate = toZonedTime(now, preferencesForm.timezone)
    return format(zonedDate, preferencesForm.dateFormat)
  } catch (error) {
    return 'Invalid format'
  }
})

const formatTimeExample = computed(() => {
  try {
    // Use date-fns-tz to handle timezone
    const zonedDate = toZonedTime(now, preferencesForm.timezone)
    return format(zonedDate, preferencesForm.timeFormat)
  } catch (error) {
    return 'Invalid format'
  }
})

const formatDateTimeExample = computed(() => {
  try {
    // Use date-fns-tz to handle timezone
    const zonedDate = toZonedTime(now, preferencesForm.timezone)
    return format(zonedDate, `${preferencesForm.dateFormat} ${preferencesForm.timeFormat}`)
  } catch (error) {
    return 'Invalid format'
  }
})

onMounted(() => {
  // Initialize form with user data
  if (authStore.user) {
    profileForm.displayName = authStore.user.displayName || ''
    profileForm.email = authStore.user.email || ''
  }
  
  // Initialize preferences form
  preferencesForm.dateFormat = preferencesStore.dateFormat
  preferencesForm.timeFormat = preferencesStore.timeFormat
  preferencesForm.timezone = preferencesStore.timezone
})

const showToastNotification = (message, type = 'success') => {
  toastMessage.value = message
  toastType.value = type
  showToast.value = true
  
  // Auto-hide toast after 3 seconds
  setTimeout(() => {
    hideToast()
  }, 3000)
}

const hideToast = () => {
  showToast.value = false
}

const saveProfile = async () => {
  if (!auth.currentUser) {
    showToastNotification('You must be logged in to update your profile', 'error')
    return
  }

  try {
    isLoading.value = true
    
    // Check if display name has changed
    if (profileForm.displayName !== auth.currentUser.displayName) {
      await updateProfile(auth.currentUser, {
        displayName: profileForm.displayName
      })
      
      // Update local user state
      if (authStore.user) {
        authStore.user.displayName = profileForm.displayName
      }
      
      showToastNotification('Profile updated successfully', 'success')
    } else {
      showToastNotification('No changes to save', 'success')
    }
  } catch (error) {
    console.error('Error updating profile:', error)
    showToastNotification('Failed to update profile: ' + error.message, 'error')
  } finally {
    isLoading.value = false
  }
}

const savePreferences = async () => {
  try {
    isPreferencesLoading.value = true
    
    // Update preferences in store
    preferencesStore.setDateFormat(preferencesForm.dateFormat)
    preferencesStore.setTimeFormat(preferencesForm.timeFormat)
    preferencesStore.setTimezone(preferencesForm.timezone)
    
    showToastNotification('Preferences saved successfully', 'success')
  } catch (error) {
    console.error('Error saving preferences:', error)
    showToastNotification('Failed to save preferences', 'error')
  } finally {
    isPreferencesLoading.value = false
  }
}

const resetPreferences = () => {
  preferencesStore.resetToDefaults()
  preferencesForm.dateFormat = preferencesStore.dateFormat
  preferencesForm.timeFormat = preferencesStore.timeFormat
  showToastNotification('Preferences reset to defaults', 'success')
}
</script>
