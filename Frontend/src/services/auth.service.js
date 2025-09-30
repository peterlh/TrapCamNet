import { auth } from '@/firebase/config';
import { 
  signInWithEmailAndPassword,
  createUserWithEmailAndPassword,
  signOut,
  onAuthStateChanged,
  GoogleAuthProvider,
  signInWithPopup,
  signInWithRedirect,
  getRedirectResult,
  GithubAuthProvider,
  TwitterAuthProvider,
  FacebookAuthProvider,
  browserLocalPersistence,
  setPersistence
} from 'firebase/auth';

class AuthService {
  // Login with email and password
  async login(email, password) {
    try {
      // Set persistence to LOCAL to persist the user's session
      await setPersistence(auth, browserLocalPersistence);
      
      const userCredential = await signInWithEmailAndPassword(auth, email, password);
      const user = userCredential.user;
      
      // Get the Firebase ID token for backend authentication
      const token = await user.getIdToken();
      
      // Store the token in localStorage
      localStorage.setItem('auth_token', token);
      
      console.log('Login successful, token stored');
      
      return {
        user,
        token
      };
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  }

  // Login with Google
  async loginWithGoogle() {
    try {
      const provider = new GoogleAuthProvider();
      // Add custom parameters for better compatibility
      provider.setCustomParameters({
        prompt: 'select_account'
      });
      
      // Use signInWithPopup instead of redirect to avoid issues with Firebase hosting
      const result = await signInWithPopup(auth, provider);
      const user = result.user;
      
      // Get the Firebase ID token for backend authentication
      const token = await user.getIdToken();
      
      // Store the token in localStorage
      localStorage.setItem('auth_token', token);
      
      console.log('Google login successful, token stored');
      
      return {
        user,
        token
      };
    } catch (error) {
      console.error('Google login error:', error);
      throw error;
    }
  }

  // Login with GitHub
  async loginWithGithub() {
    try {
      const provider = new GithubAuthProvider();
      // Add custom parameters for better compatibility
      provider.setCustomParameters({
        allow_signup: 'false'
      });
      
      // Use signInWithPopup instead of redirect to avoid issues with Firebase hosting
      const result = await signInWithPopup(auth, provider);
      const user = result.user;
      
      // Get the Firebase ID token for backend authentication
      const token = await user.getIdToken();
      
      // Store the token in localStorage
      localStorage.setItem('auth_token', token);
      
      console.log('GitHub login successful, token stored');
      
      return {
        user,
        token
      };
    } catch (error) {
      console.error('GitHub login error:', error);
      throw error;
    }
  }

  // Login with Twitter
  async loginWithTwitter() {
    try {
      const provider = new TwitterAuthProvider();
      
      // Use signInWithPopup instead of redirect to avoid issues with Firebase hosting
      const result = await signInWithPopup(auth, provider);
      const user = result.user;
      
      // Get the Firebase ID token for backend authentication
      const token = await user.getIdToken();
      
      // Store the token in localStorage
      localStorage.setItem('auth_token', token);
      
      console.log('Twitter login successful, token stored');
      
      return {
        user,
        token
      };
    } catch (error) {
      console.error('Twitter login error:', error);
      throw error;
    }
  }

  // Login with Facebook
  async loginWithFacebook() {
    try {
      const provider = new FacebookAuthProvider();
      // Add custom parameters for better compatibility
      provider.setCustomParameters({
        display: 'popup'
      });
      
      // Use signInWithPopup instead of redirect to avoid issues with Firebase hosting
      const result = await signInWithPopup(auth, provider);
      const user = result.user;
      
      // Get the Firebase ID token for backend authentication
      const token = await user.getIdToken();
      
      // Store the token in localStorage
      localStorage.setItem('auth_token', token);
      
      console.log('Facebook login successful, token stored');
      
      return {
        user,
        token
      };
    } catch (error) {
      console.error('Facebook login error:', error);
      throw error;
    }
  }

  // Register a new user
  async register(email, password) {
    try {
      const userCredential = await createUserWithEmailAndPassword(auth, email, password);
      const user = userCredential.user;
      
      // Get the Firebase ID token for backend authentication
      const token = await user.getIdToken();
      
      // Store the token in localStorage
      localStorage.setItem('auth_token', token);
      
      return {
        user,
        token
      };
    } catch (error) {
      console.error('Registration error:', error);
      throw error;
    }
  }

  // Logout the current user
  async logout() {
    try {
      await signOut(auth);
      localStorage.removeItem('auth_token');
    } catch (error) {
      console.error('Logout error:', error);
      throw error;
    }
  }

  // Get the current authentication token
  getAuthToken() {
    return localStorage.getItem('auth_token');
  }

  // Check if user is authenticated
  isAuthenticated() {
    return !!this.getAuthToken();
  }

  // Set up an observer for authentication state changes
  onAuthStateChange(callback) {
    return onAuthStateChanged(auth, async (user) => {
      console.log('Auth state changed:', user ? `User ${user.email} signed in` : 'User signed out');
      
      if (user) {
        // User is signed in
        try {
          // Refresh the token
          const token = await user.getIdToken(true);
          localStorage.setItem('auth_token', token);
          console.log('Token refreshed and stored in localStorage');
          
          // We no longer need to check for redirect result since we're using popup
        } catch (error) {
          console.error('Error refreshing token:', error);
        }
      } else {
        // User is signed out
        localStorage.removeItem('auth_token');
        console.log('Token removed from localStorage');
      }
      
      // Call the provided callback with the user object
      callback(user);
    });
  }
}

export default new AuthService();
