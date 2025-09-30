import { format, parseISO, isValid } from 'date-fns';
import { toZonedTime } from 'date-fns-tz';
import { usePreferencesStore } from '@/stores/preferences.store';

/**
 * Format a date using the user's preferred date format and timezone
 * @param {Date|string|number} date - The date to format
 * @param {string} [customFormat] - Optional custom format to override user preference
 * @param {string} [defaultText=''] - Text to return if date is null/undefined
 * @returns {string} Formatted date string
 */
export function formatDate(date, customFormat = null, defaultText = '') {
  try {
    // Return default text if date is null or undefined
    if (date === null || date === undefined) {
      return defaultText;
    }
    
    const preferences = usePreferencesStore();
    const formatString = customFormat || preferences.dateFormat;
    
    // Handle different input types
    let validDate = date instanceof Date ? date : parseISO(date);
    
    if (!isValid(validDate)) {
      return defaultText || 'Invalid date';
    }
    
    // Convert to user's preferred timezone
    validDate = toZonedTime(validDate, preferences.timezone);
    
    return format(validDate, formatString);
  } catch (error) {
    console.error('Error formatting date:', error);
    return defaultText || 'Invalid date';
  }
}

/**
 * Format a time using the user's preferred time format and timezone
 * @param {Date|string|number} date - The date to format
 * @param {string} [customFormat] - Optional custom format to override user preference
 * @param {string} [defaultText=''] - Text to return if date is null/undefined
 * @returns {string} Formatted time string
 */
export function formatTime(date, customFormat = null, defaultText = '') {
  try {
    // Return default text if date is null or undefined
    if (date === null || date === undefined) {
      return defaultText;
    }
    
    const preferences = usePreferencesStore();
    const formatString = customFormat || preferences.timeFormat;
    
    // Handle different input types
    let validDate = date instanceof Date ? date : parseISO(date);
    
    if (!isValid(validDate)) {
      return defaultText || 'Invalid time';
    }
    
    // Convert to user's preferred timezone
    validDate = toZonedTime(validDate, preferences.timezone);
    
    return format(validDate, formatString);
  } catch (error) {
    console.error('Error formatting time:', error);
    return defaultText || 'Invalid time';
  }
}

/**
 * Format a date and time using the user's preferred formats and timezone
 * @param {Date|string|number} date - The date to format
 * @param {string} [customFormat] - Optional custom format to override user preference
 * @param {string} [defaultText=''] - Text to return if date is null/undefined
 * @returns {string} Formatted date and time string
 */
export function formatDateTime(date, customFormat = null, defaultText = '') {
  try {
    // Return default text if date is null or undefined
    if (date === null || date === undefined) {
      return defaultText;
    }
    
    const preferences = usePreferencesStore();
    const formatString = customFormat || preferences.dateTimeFormat;
    
    // Handle different input types
    let validDate = date instanceof Date ? date : parseISO(date);
    
    if (!isValid(validDate)) {
      return defaultText || 'Invalid date/time';
    }
    
    // Convert to user's preferred timezone
    validDate = toZonedTime(validDate, preferences.timezone);
    
    return format(validDate, formatString);
  } catch (error) {
    console.error('Error formatting date/time:', error);
    return defaultText || 'Invalid date/time';
  }
}

/**
 * Convert a date to the user's preferred timezone
 * @param {Date|string|number} date - The date to convert
 * @param {string} [targetTimezone] - Optional timezone to override user preference
 * @returns {Date} Date object in the target timezone
 */
export function convertToTimezone(date, targetTimezone = null) {
  try {
    if (date === null || date === undefined) {
      return null;
    }
    
    const preferences = usePreferencesStore();
    const timezone = targetTimezone || preferences.timezone;
    
    // Handle different input types
    const validDate = date instanceof Date ? date : parseISO(date);
    
    if (!isValid(validDate)) {
      return null;
    }
    
    // Convert to specified timezone
    return toZonedTime(validDate, timezone);
  } catch (error) {
    console.error('Error converting timezone:', error);
    return null;
  }
}
