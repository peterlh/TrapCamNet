<template>
  <CToaster class="position-fixed top-0 end-0 p-3" placement="top-end">
    <CToast
      v-for="toast in toasts"
      :key="toast.id"
      :autohide="toast.autohide"
      :delay="toast.delay"
      :color="toast.color"
      @close="removeToast(toast.id)"
    >
      <CToastHeader closeButton>
        <strong>{{ toast.title }}</strong>
      </CToastHeader>
      <CToastBody>{{ toast.message }}</CToastBody>
    </CToast>
  </CToaster>
</template>

<script>
import { useToast } from '@/composables/toast'

export default {
  name: 'AppToast',
  setup() {
    const { toasts, removeToast } = useToast()
    
    return {
      toasts,
      removeToast
    }
  }
}
</script>
