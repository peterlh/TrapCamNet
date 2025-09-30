<template>
  <div class="bwrapper min-vh-100 d-flex flex-row align-items-center">
    <CContainer>
      <CRow class="justify-content-center">
        <CCol :md="9" :lg="7" :xl="6">
          <CCard class="mx-4">
            <CCardBody class="p-4">
              <CForm @submit.prevent="handleRegister">
                <h1>TrapCam Register</h1>
                <p class="text-body-secondary">Create your account</p>
                <CAlert v-if="error" color="danger">{{ error }}</CAlert>
                <CInputGroup class="mb-3">
                  <CInputGroupText>
                    <CIcon icon="cil-user" />
                  </CInputGroupText>
                  <CFormInput 
                    v-model="displayName" 
                    placeholder="Full Name" 
                    autocomplete="name" 
                    required
                  />
                </CInputGroup>
                <CInputGroup class="mb-3">
                  <CInputGroupText>@</CInputGroupText>
                  <CFormInput 
                    v-model="email" 
                    type="email" 
                    placeholder="Email" 
                    autocomplete="email" 
                    required
                  />
                </CInputGroup>
                <CInputGroup class="mb-3">
                  <CInputGroupText>
                    <CIcon icon="cil-lock-locked" />
                  </CInputGroupText>
                  <CFormInput
                    v-model="password"
                    type="password"
                    placeholder="Password"
                    autocomplete="new-password"
                    required
                    minlength="6"
                  />
                </CInputGroup>
                <CInputGroup class="mb-4">
                  <CInputGroupText>
                    <CIcon icon="cil-lock-locked" />
                  </CInputGroupText>
                  <CFormInput
                    v-model="confirmPassword"
                    type="password"
                    placeholder="Confirm password"
                    autocomplete="new-password"
                    required
                  />
                </CInputGroup>
                <div class="d-grid gap-2">
                  <CButton type="submit" color="success" :disabled="loading">
                    {{ loading ? 'Creating Account...' : 'Create Account' }}
                  </CButton>
                  <CButton color="link" @click="goToLogin">Already have an account? Login</CButton>
                </div>
              </CForm>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </CContainer>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const router = useRouter()
const authStore = useAuthStore()

const displayName = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const loading = ref(false)
const error = ref(null)

const passwordsMatch = computed(() => {
  return password.value === confirmPassword.value
})

async function handleRegister() {
  error.value = null
  
  if (!passwordsMatch.value) {
    error.value = 'Passwords do not match'
    return
  }
  
  loading.value = true
  
  try {
    await authStore.register(email.value, password.value)
    // Update profile with display name
    if (authStore.user) {
      await authStore.user.updateProfile({
        displayName: displayName.value
      })
    }
    router.push('/dashboard')
  } catch (err) {
    console.error('Registration error:', err)
    error.value = err.message || 'Failed to create account. Please try again.'
  } finally {
    loading.value = false
  }
}

function goToLogin() {
  router.push('/pages/login')
}
</script>
