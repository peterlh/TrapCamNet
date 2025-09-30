import { getMessaging, getToken, onMessage } from 'firebase/messaging'
import { firebaseApp } from './config'

// Initialize Firebase messaging
const messaging = getMessaging(firebaseApp)

/**
 * Request permission and get FCM token
 * @returns {Promise<string|null>} FCM token or null if permission denied
 */
export const requestNotificationPermission = async () => {
  try {
    // Request permission
    const permission = await Notification.requestPermission()
    
    if (permission !== 'granted') {
      console.log('Notification permission denied')
      return null
    }
    
    // Get FCM token
    const currentToken = await getToken(messaging, {
      vapidKey: import.meta.env.VITE_FIREBASE_VAPID_KEY
    })
    
    if (currentToken) {
      console.log('FCM token obtained:', currentToken)
      return currentToken
    } else {
      console.log('No registration token available')
      return null
    }
  } catch (error) {
    console.error('Error getting FCM token:', error)
    return null
  }
}

/**
 * Set up foreground message handler
 * @param {Function} callback Function to call when a message is received
 */
export const setupMessageListener = (callback) => {
  onMessage(messaging, (payload) => {
    console.log('Message received in foreground:', payload)
    
    // Create notification options
    const notificationOptions = {
      body: payload.notification?.body || 'New notification',
      icon: '/img/icons/android-chrome-192x192.png'
    }
    
    // Show notification
    if ('Notification' in window && Notification.permission === 'granted') {
      const notification = new Notification(
        payload.notification?.title || 'TrapCam Notification', 
        notificationOptions
      )
      
      // Handle notification click
      notification.onclick = () => {
        notification.close()
        window.focus()
        
        // Call the callback with the payload data
        if (callback && typeof callback === 'function') {
          callback(payload.data)
        }
      }
    }
  })
}
