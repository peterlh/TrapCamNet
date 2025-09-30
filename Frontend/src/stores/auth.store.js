import { defineStore } from 'pinia';
import authService from '@/services/auth.service';
import { ref, computed } from 'vue';

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref(null);
  const loading = ref(true);
  const error = ref(null);

  // Getters
  const isAuthenticated = computed(() => !!user.value);
  const userEmail = computed(() => user.value?.email || '');

  // Actions
  async function login(email, password) {
    try {
      loading.value = true;
      error.value = null;
      const result = await authService.login(email, password);
      user.value = result.user;
      return result;
    } catch (err) {
      error.value = err.message || 'Failed to login';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function register(email, password) {
    try {
      loading.value = true;
      error.value = null;
      const result = await authService.register(email, password);
      user.value = result.user;
      return result;
    } catch (err) {
      error.value = err.message || 'Failed to register';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function loginWithGoogle() {
    try {
      loading.value = true;
      error.value = null;
      // Using popup now, so we can handle the result directly
      const result = await authService.loginWithGoogle();
      user.value = result.user;
      loading.value = false;
      return result;
    } catch (err) {
      error.value = err.message || 'Failed to login with Google';
      loading.value = false;
      throw err;
    }
  }

  async function loginWithGithub() {
    try {
      loading.value = true;
      error.value = null;
      // Using popup now, so we can handle the result directly
      const result = await authService.loginWithGithub();
      user.value = result.user;
      loading.value = false;
      return result;
    } catch (err) {
      error.value = err.message || 'Failed to login with GitHub';
      loading.value = false;
      throw err;
    }
  }

  async function loginWithTwitter() {
    try {
      loading.value = true;
      error.value = null;
      // Using popup now, so we can handle the result directly
      const result = await authService.loginWithTwitter();
      user.value = result.user;
      loading.value = false;
      return result;
    } catch (err) {
      error.value = err.message || 'Failed to login with Twitter';
      loading.value = false;
      throw err;
    }
  }

  async function loginWithFacebook() {
    try {
      loading.value = true;
      error.value = null;
      // Using popup now, so we can handle the result directly
      const result = await authService.loginWithFacebook();
      user.value = result.user;
      loading.value = false;
      return result;
    } catch (err) {
      error.value = err.message || 'Failed to login with Facebook';
      loading.value = false;
      throw err;
    }
  }

  async function logout() {
    try {
      loading.value = true;
      await authService.logout();
      user.value = null;
    } catch (err) {
      error.value = err.message || 'Failed to logout';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  function setUser(newUser) {
    user.value = newUser;
  }

  // Initialize auth state
  function init() {
    loading.value = true;
    console.log('Initializing auth state...');
    const unsubscribe = authService.onAuthStateChange((firebaseUser) => {
      console.log('Auth state change detected in store:', firebaseUser ? 'User authenticated' : 'No user');
      user.value = firebaseUser;
      loading.value = false;
      
      // If the user is authenticated, ensure token is stored properly
      if (firebaseUser) {
        // Get the token and store it
        firebaseUser.getIdToken().then(token => {
          localStorage.setItem('auth_token', token);
          console.log('Token stored in localStorage from auth store init');
          
          // Use router navigation instead of direct hash manipulation
          // This will trigger the navigation guards properly
          const currentPath = window.location.hash;
          if (currentPath.includes('/pages/login') || currentPath.includes('/pages/register')) {
            console.log('On login/register page while authenticated, redirecting to dashboard');
            // Use a small timeout to ensure the router is ready
            setTimeout(() => {
              window.location.href = '/#/dashboard';
            }, 100);
          }
        }).catch(err => {
          console.error('Error getting token:', err);
        });
      }
    });
    return unsubscribe;
  }

  // Add getToken method to retrieve the authentication token
  async function getToken() {
    console.log('Getting token from auth store');
    
    // Check if we have a valid token in localStorage first
    const storedToken = localStorage.getItem('auth_token');
    const tokenExpiry = localStorage.getItem('auth_token_expiry');
    const currentTime = Date.now();
    
    // Buffer time (5 minutes) before expiration to refresh token
    const bufferTime = 5 * 60 * 1000;
    
    // If we have a token and it's not expired (with buffer time)
    if (storedToken && tokenExpiry && currentTime < parseInt(tokenExpiry) - bufferTime) {
      console.log('Using valid token from localStorage');
      return storedToken;
    }
    
    // If token is expired or we don't have one, and we have a user
    if (user.value) {
      try {
        // Get a fresh token from Firebase
        const token = await user.value.getIdToken(true);
        console.log('Got fresh token from Firebase');
        
        // Store token and its expiration time (default Firebase token lifetime is 3600 seconds = 1 hour)
        const expiryTime = currentTime + (3600 * 1000);
        localStorage.setItem('auth_token', token);
        localStorage.setItem('auth_token_expiry', expiryTime.toString());
        
        return token;
      } catch (err) {
        console.error('Error getting fresh token:', err);
        
        // If we have a stored token but it's expired, return it as last resort
        if (storedToken) {
          console.log('Using expired token from localStorage as fallback');
          return storedToken;
        }
      }
    }
    
    // Fall back to token in localStorage even if we couldn't validate expiry
    console.log('Using token from localStorage:', storedToken ? 'Token exists' : 'No token found');
    return storedToken;
  }

  return {
    // State
    user,
    loading,
    error,

    // Getters
    isAuthenticated,
    userEmail,

    // Actions
    login,
    register,
    logout,
    setUser,
    init,
    loginWithGoogle,
    loginWithGithub,
    loginWithTwitter,
    loginWithFacebook,
    getToken
  };
});
