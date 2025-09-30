<template>
  <div class="main-nav-links">
    <router-link to="/dashboard" class="nav-link d-flex align-items-center">
      <span class="nav-icon"><i class="cil-speedometer"></i></span>
      <span>Dashboard</span>
    </router-link>
    
    <router-link to="/cameras" class="nav-link d-flex align-items-center">
      <span class="nav-icon"><i class="cil-camera"></i></span>
      <span>Cameras</span>
    </router-link>
    
    <router-link to="/locations" class="nav-link d-flex align-items-center">
      <span class="nav-icon"><i class="cil-map"></i></span>
      <span>Locations</span>
    </router-link>
  </div>
</template>

<script setup>
// No imports needed
</script>

<style scoped>
.main-nav-links {
  padding: 0.5rem 0;
}

.nav-link {
  padding: 0.8rem 1rem;
  color: rgba(255, 255, 255, 0.6);
  text-decoration: none;
  transition: all 0.2s ease;
}

.nav-link:hover {
  color: #fff;
  background-color: rgba(255, 255, 255, 0.1);
}

.nav-link.router-link-active {
  color: #fff;
  background-color: rgba(255, 255, 255, 0.15);
}

.nav-icon {
  margin-right: 0.5rem;
}
</style>
