import { ref, provide, inject } from 'vue'

const toastSymbol = Symbol()

export function provideToast() {
  const toasts = ref([])
  
  function addToast({ title, message, color = 'primary', autohide = true, delay = 5000 }) {
    const id = Date.now()
    toasts.value.push({ id, title, message, color, autohide, delay })
    
    if (autohide) {
      setTimeout(() => {
        removeToast(id)
      }, delay)
    }
    
    return id
  }
  
  function removeToast(id) {
    const index = toasts.value.findIndex(toast => toast.id === id)
    if (index !== -1) {
      toasts.value.splice(index, 1)
    }
  }
  
  provide(toastSymbol, {
    toasts,
    addToast,
    removeToast
  })
  
  return {
    toasts,
    addToast,
    removeToast
  }
}

export function useToast() {
  const toast = inject(toastSymbol)
  
  if (!toast) {
    throw new Error('Toast provider not found')
  }
  
  return toast
}
