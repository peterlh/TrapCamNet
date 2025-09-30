<template>
  <div class="wrapper min-vh-100 d-flex flex-row align-items-center">
    <CContainer>
      <CRow class="justify-content-center">
        <CCol :md="8">
          <CCardGroup>
            <CCard class="p-4">
              <CCardBody>
                <CForm @submit.prevent="handleLogin">
                  <h1>TrapCam Login</h1>
                  <p class="text-body-secondary">Sign In to your account</p>
                  <CAlert v-if="error" color="danger">{{ error }}</CAlert>
                  <CInputGroup class="mb-3">
                    <CInputGroupText>
                      <CIcon icon="cil-user" />
                    </CInputGroupText>
                    <CFormInput
                      v-model="email"
                      placeholder="Email"
                      type="email"
                      autocomplete="email"
                      required
                    />
                  </CInputGroup>
                  <CInputGroup class="mb-4">
                    <CInputGroupText>
                      <CIcon icon="cil-lock-locked" />
                    </CInputGroupText>
                    <CFormInput
                      v-model="password"
                      type="password"
                      placeholder="Password"
                      autocomplete="current-password"
                      required
                    />
                  </CInputGroup>
                  <CRow>
                    <CCol :xs="6">
                      <CButton type="submit" color="primary" class="px-4" :disabled="loading">
                        {{ loading ? 'Logging in...' : 'Login' }}
                      </CButton>
                    </CCol>
                    <CCol :xs="6" class="text-right">
                      <CButton color="link" class="px-0" @click="forgotPassword">
                        Forgot password?
                      </CButton>
                    </CCol>
                  </CRow>
                  
                  <div class="mt-4">
                    <p class="text-center mb-3">Or sign in with</p>
                    <CRow>
                      <CCol>
                        <CButton color="danger" class="w-100 mb-2" @click="handleGoogleLogin" :disabled="loading">
                          <CIcon icon="cib-google" class="me-2" /> Google
                        </CButton>
                      </CCol>
                    </CRow>
                    <CRow>
                      <CCol md="6">
                        <CButton color="dark" class="w-100 mb-2" @click="handleGithubLogin" :disabled="loading">
                          <CIcon icon="cib-github" class="me-2" /> GitHub
                        </CButton>
                      </CCol>
                      <CCol md="6">
                        <CButton color="info" class="w-100 mb-2" @click="handleTwitterLogin" :disabled="loading">
                          <CIcon icon="cib-twitter" class="me-2" /> Twitter
                        </CButton>
                      </CCol>
                    </CRow>
                    <CRow>
                      <CCol>
                        <CButton color="primary" variant="outline" class="w-100" @click="handleFacebookLogin" :disabled="loading">
                          <CIcon icon="cib-facebook" class="me-2" /> Facebook
                        </CButton>
                      </CCol>
                    </CRow>
                  </div>
                </CForm>
              </CCardBody>
            </CCard>
            <CCard class="text-white bg-primary py-5" style="width: 44%">
              <CCardBody class="text-center">
                <div>
                  <h2>TrapCam System</h2>
                  <p>
                    Access the TrapCam dashboard to view and manage your camera trap images.
                    New users should contact the administrator for account access.
                  </p>
                  <CButton color="light" variant="outline" class="mt-3" @click="goToRegister">
                    Register Now!
                  </CButton>
                </div>
              </CCardBody>
            </CCard>
          </CCardGroup>
        </CCol>
      </CRow>
    </CContainer>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const loading = ref(false)
const error = ref(null)

async function handleLogin() {
  error.value = null
  loading.value = true
  
  try {
    await authStore.login(email.value, password.value)
    router.push('/dashboard')
  } catch (err) {
    console.error('Login error:', err)
    error.value = 'Invalid email or password. Please try again.'
  } finally {
    loading.value = false
  }
}

async function handleGoogleLogin() {
  error.value = null;
  loading.value = true;
  
  try {
    // Using popup now, so we can handle the result directly
    await authStore.loginWithGoogle();
    console.log('Google login successful, redirecting to dashboard');
    router.push('/dashboard');
  } catch (err) {
    console.error('Google login error:', err);
    error.value = 'Failed to login with Google. Please try again.';
    loading.value = false;
  }
}

async function handleGithubLogin() {
  error.value = null;
  loading.value = true;
  
  try {
    // Using popup now, so we can handle the result directly
    await authStore.loginWithGithub();
    console.log('GitHub login successful, redirecting to dashboard');
    router.push('/dashboard');
  } catch (err) {
    console.error('GitHub login error:', err);
    error.value = 'Failed to login with GitHub. Please try again.';
    loading.value = false;
  }
}

async function handleTwitterLogin() {
  error.value = null;
  loading.value = true;
  
  try {
    // Using popup now, so we can handle the result directly
    await authStore.loginWithTwitter();
    console.log('Twitter login successful, redirecting to dashboard');
    router.push('/dashboard');
  } catch (err) {
    console.error('Twitter login error:', err);
    error.value = 'Failed to login with Twitter. Please try again.';
    loading.value = false;
  }
}

async function handleFacebookLogin() {
  error.value = null;
  loading.value = true;
  
  try {
    // Using popup now, so we can handle the result directly
    await authStore.loginWithFacebook();
    console.log('Facebook login successful, redirecting to dashboard');
    router.push('/dashboard');
  } catch (err) {
    console.error('Facebook login error:', err);
    error.value = 'Failed to login with Facebook. Please try again.';
    loading.value = false;
  }
}

function forgotPassword() {
  // This would typically trigger a password reset flow
  alert('Please contact your administrator to reset your password.')
}

function goToRegister() {
  router.push('/pages/register')
}
</script>
