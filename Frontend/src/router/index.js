import { h, resolveComponent } from 'vue'
import { createRouter, createWebHashHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

import DefaultLayout from '@/layouts/DefaultLayout'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: DefaultLayout,
    redirect: '/dashboard',
    children: [
      {
        path: '/dashboard',
        name: 'Dashboard',
        // route level code-splitting
        // this generates a separate chunk (about.[hash].js) for this route
        // which is lazy-loaded when the route is visited.
        component: () =>
          import(
            /* webpackChunkName: "dashboard" */ '@/views/dashboard/Dashboard.vue'
          ),
      },
      {
        path: '/cameras',
        name: 'Cameras',
        component: () =>
          import(
            /* webpackChunkName: "cameras" */ '@/views/cameras/Cameras.vue'
          ),
      },
      {
        path: '/cameras/:id',
        name: 'CameraDetail',
        component: () =>
          import(
            /* webpackChunkName: "camera-detail" */ '@/views/cameras/CameraDetail.vue'
          ),
      },
      {
        path: '/cameras/:id/email-archive',
        name: 'EmailArchive',
        component: () =>
          import(
            /* webpackChunkName: "email-archive" */ '@/views/cameras/EmailArchive.vue'
          ),
      },
      {
        path: '/locations',
        name: 'Locations',
        component: () =>
          import(
            /* webpackChunkName: "locations" */ '@/views/locations/Locations.vue'
          ),
      },
      {
        path: '/images',
        name: 'Images',
        component: () =>
          import(
            /* webpackChunkName: "images" */ '@/views/images/Images.vue'
          ),
      },
      {
        path: '/profile',
        name: 'Profile',
        component: () =>
          import(
            /* webpackChunkName: "profile" */ '@/views/profile/Profile.vue'
          ),
      },
      {
        path: '/settings/notifications',
        name: 'Notifications',
        component: () =>
          import(
            /* webpackChunkName: "notifications" */ '@/views/settings/Notifications.vue'
          ),
      },
    ],
  },
  {
    path: '/404',
    name: 'Page404',
    component: () => import('@/views/pages/Page404'),
  },
  {
    path: '/500',
    name: 'Page500',
    component: () => import('@/views/pages/Page500'),
  },
]

// Add a catch-all route that redirects to the 404 page
routes.push({
  path: '/:pathMatch(.*)*',
  redirect: '/404'
})

const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior() {
    // always scroll to top
    return { top: 0 }
  },
})

// Navigation guards
router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()
  const publicPages = ['/pages/login', '/pages/register', '/404', '/500']
  const authRequired = !publicPages.includes(to.path)
  
  console.log(`Navigation: ${from.path} -> ${to.path}, auth required: ${authRequired}, auth state: ${authStore.isAuthenticated ? 'authenticated' : 'not authenticated'}, loading: ${authStore.loading}`)
  
  // If we're still loading the auth state, wait for it with a timeout
  if (authStore.loading) {
    console.log('Auth state is still loading, waiting...')
    
    // Use a promise with timeout to wait for auth state
    const waitForAuth = new Promise((resolve) => {
      // Check auth state every 100ms
      const checkInterval = setInterval(() => {
        if (!authStore.loading) {
          clearInterval(checkInterval)
          clearTimeout(timeoutId)
          resolve(true)
        }
      }, 100)
      
      // Set a maximum wait time of 2 seconds
      const timeoutId = setTimeout(() => {
        clearInterval(checkInterval)
        resolve(false)
      }, 2000)
    })
    
    const authLoaded = await waitForAuth
    console.log(`Auth state loaded: ${authLoaded}, authenticated: ${authStore.isAuthenticated}`)
    
    // If auth state is still loading after timeout, make a decision based on localStorage
    if (!authLoaded) {
      const hasToken = !!localStorage.getItem('auth_token')
      console.log(`Auth state still loading after timeout, token in localStorage: ${hasToken}`)
      
      if (authRequired && !hasToken) {
        console.log('No token found, redirecting to login')
        return next('/pages/login')
      } else if (!authRequired && hasToken) {
        // If trying to access login page with a token, go to dashboard
        if (to.path === '/pages/login' || to.path === '/pages/register') {
          console.log('Token found while accessing login page, redirecting to dashboard')
          return next('/dashboard')
        }
      }
    }
  }
  
  // Check if route requires authentication
  if (authRequired && !authStore.isAuthenticated) {
    console.log('Authentication required but not authenticated, redirecting to login')
    // Redirect to login page
    return next('/pages/login')
  }
  
  // If on login page and already authenticated, redirect to dashboard
  if ((to.path === '/pages/login' || to.path === '/pages/register') && authStore.isAuthenticated) {
    console.log('Already authenticated, redirecting to dashboard')
    return next('/dashboard')
  }
  
  // Proceed with navigation
  console.log(`Proceeding with navigation to ${to.path}`)
  next()
})

export default router
