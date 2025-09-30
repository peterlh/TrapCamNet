import { defineStore } from 'pinia';
import apiService from '@/services/api.service';
import { ref, computed } from 'vue';

export const useImagesStore = defineStore('images', () => {
  // State
  const images = ref([]);
  const loading = ref(false);
  const error = ref(null);
  const totalCount = ref(0);
  const camerasCount = ref(0);
  const currentPage = ref(1);
  const pageSize = ref(12);
  const totalPages = ref(1);

  // Getters
  const hasImages = computed(() => images.value.length > 0);
  const imagesCount = computed(() => totalCount.value);

  // Actions
  async function fetchImages() {
    try {
      loading.value = true;
      error.value = null;
      const data = await apiService.getImages();
      images.value = data;
      totalCount.value = data.length;
      camerasCount.value = 0; // Not available in this API
      return data;
    } catch (err) {
      error.value = err.message || 'Failed to fetch images';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function fetchImagesWithDetails(page = 1, size = 12) {
    try {
      loading.value = true;
      error.value = null;
      currentPage.value = page;
      pageSize.value = size;
      
      const response = await apiService.get(`/images/details?page=${page}&pageSize=${size}`);
      
      images.value = response.images || [];
      totalCount.value = response.totalCount || 0;
      camerasCount.value = response.camerasCount || 0;
      totalPages.value = response.totalPages || 1;
      
      return response;
    } catch (err) {
      error.value = err.message || 'Failed to fetch images';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  function clearImages() {
    images.value = [];
    totalCount.value = 0;
    camerasCount.value = 0;
  }

  return {
    // State
    images,
    loading,
    error,
    totalCount,
    camerasCount,
    currentPage,
    pageSize,
    totalPages,
    
    // Getters
    hasImages,
    imagesCount,
    
    // Actions
    fetchImages,
    fetchImagesWithDetails,
    clearImages
  };
});
