<script setup>
import { onMounted, ref, computed } from 'vue'
import { useColorModes } from '@coreui/vue'
import { useRouter } from 'vue-router'

import AppBreadcrumb from '@/components/AppBreadcrumb.vue'
import { useSidebarStore } from '@/stores/sidebar.js'
import { useAuthStore } from '@/stores/auth.store'
import defaultAvatarImg from '@/assets/images/avatars/8.jpg'

const router = useRouter()
const authStore = useAuthStore()
const headerClassNames = ref('mb-4 p-0')
const { colorMode, setColorMode } = useColorModes('coreui-free-vue-admin-template-theme')
const sidebar = useSidebarStore()
const defaultAvatar = defaultAvatarImg

// User information
const userName = computed(() => authStore.user?.displayName || 'User')
const userPhotoURL = computed(() => authStore.user?.photoURL || null)

// Logout handler
const handleLogout = async () => {
  try {
    await authStore.logout()
    router.push('/pages/login')
  } catch (error) {
    console.error('Logout failed:', error)
  }
}

onMounted(() => {
  document.addEventListener('scroll', () => {
    if (document.documentElement.scrollTop > 0) {
      headerClassNames.value = 'mb-4 p-0 shadow-sm'
    } else {
      headerClassNames.value = 'mb-4 p-0'
    }
  })
})
</script>

<template>
  <CHeader position="sticky" :class="headerClassNames">
    <CContainer class="border-bottom px-4" fluid>
      <CHeaderToggler @click="sidebar.toggleVisible()" style="margin-inline-start: -14px">
        <CIcon icon="cil-menu" size="lg" />
      </CHeaderToggler>
      <CHeaderNav class="d-none d-md-flex">
        <!-- Navigation items removed as requested -->
      </CHeaderNav>
      <CHeaderNav class="ms-auto">
        <!-- Theme toggle dropdown -->
        <CDropdown variant="nav-item" placement="bottom-end">
          <CDropdownToggle :caret="false">
            <CIcon v-if="colorMode === 'dark'" icon="cil-moon" size="lg" />
            <CIcon v-else-if="colorMode === 'light'" icon="cil-sun" size="lg" />
            <CIcon v-else icon="cil-contrast" size="lg" />
          </CDropdownToggle>
          <CDropdownMenu>
            <CDropdownItem
              :active="colorMode === 'light'"
              class="d-flex align-items-center"
              component="button"
              type="button"
              @click="setColorMode('light')"
            >
              <CIcon class="me-2" icon="cil-sun" size="lg" /> Light
            </CDropdownItem>
            <CDropdownItem
              :active="colorMode === 'dark'"
              class="d-flex align-items-center"
              component="button"
              type="button"
              @click="setColorMode('dark')"
            >
              <CIcon class="me-2" icon="cil-moon" size="lg" /> Dark
            </CDropdownItem>
            <CDropdownItem
              :active="colorMode === 'auto'"
              class="d-flex align-items-center"
              component="button"
              type="button"
              @click="setColorMode('auto')"
            >
              <CIcon class="me-2" icon="cil-contrast" size="lg" /> Auto
            </CDropdownItem>
          </CDropdownMenu>
        </CDropdown>
        
        <!-- User profile section -->
        <div class="d-flex align-items-center ms-3">
          <CAvatar :src="userPhotoURL || defaultAvatar" size="md" class="me-2" />
          <span class="d-none d-md-inline me-3">{{ userName }}</span>
          <CNavItem class="mx-1">
            <router-link to="/profile" class="nav-link" title="Profile">
              <CIcon icon="cil-user" size="lg" />
            </router-link>
          </CNavItem>
          <CNavItem class="mx-1">
            <a href="#" class="nav-link" @click.prevent="handleLogout" title="Log out">
              <CIcon icon="cil-lock-locked" size="lg" />
            </a>
          </CNavItem>
        </div>
      </CHeaderNav>
    </CContainer>
    <CContainer class="px-4" fluid>
      <AppBreadcrumb />
    </CContainer>
  </CHeader>
</template>
