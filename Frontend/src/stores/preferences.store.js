import { defineStore } from 'pinia';
import { ref, computed, watch } from 'vue';

// Define available date and time formats
export const DATE_FORMATS = {
  'dd-MM-yyyy': 'European with dashes (31-12-2023)',
  'yyyy-MM-dd': 'ISO (2023-12-31)',
  'dd/MM/yyyy': 'European with slashes (31/12/2023)',
  'MM/dd/yyyy': 'US (12/31/2023)',
  'dd.MM.yyyy': 'German (31.12.2023)',
  'yyyy年MM月dd日': 'Japanese (2023年12月31日)'
};

export const TIME_FORMATS = {
  'HH:mm': '24-hour (14:30)',
  'hh:mm a': '12-hour (02:30 PM)'
};

// Define available timezones
export const TIMEZONES = {
  'Europe/Copenhagen': 'Danish (Copenhagen)',
  'UTC': 'UTC',
  'Europe/London': 'UK (London)',
  'Europe/Paris': 'Central European (Paris)',
  'Europe/Berlin': 'Central European (Berlin)',
  'America/New_York': 'US Eastern (New York)',
  'America/Chicago': 'US Central (Chicago)',
  'America/Denver': 'US Mountain (Denver)',
  'America/Los_Angeles': 'US Pacific (Los Angeles)',
  'Asia/Tokyo': 'Japan (Tokyo)',
  'Australia/Sydney': 'Australia (Sydney)'
};

export const usePreferencesStore = defineStore('preferences', () => {
  // Default preferences
  const defaultDateFormat = 'dd-MM-yyyy';
  const defaultTimeFormat = 'HH:mm';
  const defaultTimezone = 'Europe/Copenhagen'; // Danish timezone as default

  // State
  const dateFormat = ref(localStorage.getItem('pref_dateFormat') || defaultDateFormat);
  const timeFormat = ref(localStorage.getItem('pref_timeFormat') || defaultTimeFormat);
  const timezone = ref(localStorage.getItem('pref_timezone') || defaultTimezone);

  // Computed properties
  const dateTimeFormat = computed(() => `${dateFormat.value} ${timeFormat.value}`);
  
  // Watch for changes and persist to localStorage
  watch(dateFormat, (newValue) => {
    localStorage.setItem('pref_dateFormat', newValue);
  });
  
  watch(timeFormat, (newValue) => {
    localStorage.setItem('pref_timeFormat', newValue);
  });
  
  watch(timezone, (newValue) => {
    localStorage.setItem('pref_timezone', newValue);
  });

  // Actions
  function setDateFormat(format) {
    if (DATE_FORMATS[format]) {
      dateFormat.value = format;
    }
  }

  function setTimeFormat(format) {
    if (TIME_FORMATS[format]) {
      timeFormat.value = format;
    }
  }

  function setTimezone(tz) {
    if (TIMEZONES[tz]) {
      timezone.value = tz;
    }
  }

  function resetToDefaults() {
    dateFormat.value = defaultDateFormat;
    timeFormat.value = defaultTimeFormat;
    timezone.value = defaultTimezone;
  }

  return {
    // State
    dateFormat,
    timeFormat,
    timezone,
    
    // Getters
    dateTimeFormat,
    
    // Actions
    setDateFormat,
    setTimeFormat,
    setTimezone,
    resetToDefaults,
    
    // Constants
    availableDateFormats: DATE_FORMATS,
    availableTimeFormats: TIME_FORMATS,
    availableTimezones: TIMEZONES
  };
});
