import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { useAuthStore } from './stores/auth.store'
import { setupMessageListener } from './firebase/messaging'

import CoreuiVue from '@coreui/vue'
import CIcon from '@coreui/icons-vue'
import { freeSet, brandSet } from '@coreui/icons'
import DocsComponents from '@/components/DocsComponents'
import DocsExample from '@/components/DocsExample'
import DocsIcons from '@/components/DocsIcons'

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.use(CoreuiVue)

app.provide('icons', { ...freeSet, ...brandSet })
app.component('CIcon', CIcon)
app.component('DocsComponents', DocsComponents)
app.component('DocsExample', DocsExample)
app.component('DocsIcons', DocsIcons)

app.mount('#app')

// Initialize auth store
const authStore = useAuthStore()
authStore.init()

// Set up Firebase messaging listener
setupMessageListener((data) => {
  console.log('Notification clicked with data:', data)
  // Navigate to the image if cameraId and imageId are provided
  if (data && data.cameraId && data.imageId) {
    router.push(`/cameras/${data.cameraId}?imageId=${data.imageId}`)
  }
})
