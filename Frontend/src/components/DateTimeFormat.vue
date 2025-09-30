<template>
  <span>{{ formattedValue }}</span>
</template>

<script setup>
import { computed } from 'vue'
import { format, parseISO, isValid } from 'date-fns'
import { usePreferencesStore } from '@/stores/preferences.store'

const props = defineProps({
  value: {
    type: [Date, String, Number, null, undefined],
    required: false,
    default: null
  },
  type: {
    type: String,
    default: 'dateTime', // 'date', 'time', or 'dateTime'
    validator: (value) => ['date', 'time', 'dateTime'].includes(value)
  },
  customFormat: {
    type: String,
    default: null
  },
  defaultText: {
    type: String,
    default: ''
  }
})

const preferencesStore = usePreferencesStore()

const formattedValue = computed(() => {
  try {
    // Return default text if value is null or undefined
    if (props.value === null || props.value === undefined) {
      return props.defaultText
    }
    
    // Handle different input types
    const dateValue = props.value instanceof Date 
      ? props.value 
      : typeof props.value === 'string' || typeof props.value === 'number'
        ? parseISO(props.value)
        : null
    
    if (!dateValue || !isValid(dateValue)) {
      return props.defaultText || 'Invalid date'
    }
    
    // Determine format based on type and user preferences
    let formatString = props.customFormat
    
    if (!formatString) {
      switch (props.type) {
        case 'date':
          formatString = preferencesStore.dateFormat
          break
        case 'time':
          formatString = preferencesStore.timeFormat
          break
        case 'dateTime':
        default:
          formatString = `${preferencesStore.dateFormat} ${preferencesStore.timeFormat}`
          break
      }
    }
    
    return format(dateValue, formatString)
  } catch (error) {
    console.error('Error formatting date:', error)
    return props.defaultText || 'Invalid date'
  }
})
</script>
