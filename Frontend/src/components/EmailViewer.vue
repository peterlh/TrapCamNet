<template>
  <iframe
    ref="iframe"
    sandbox="allow-same-origin"
    :style="{ width: '100%', border: 'none', minHeight: '500px', maxHeight: '60vh', overflow: 'auto' }"
  ></iframe>
</template>

<script setup>
import { onMounted, ref, watch } from 'vue'

const props = defineProps({
  html: String, // sanitized or raw HTML email
})

const iframe = ref(null)

const writeContent = () => {
  if (!iframe.value) return
  const doc = iframe.value.contentDocument
  if (!doc) return

  doc.open()
  doc.write(props.html)
  doc.close()
}

onMounted(() => {
  writeContent()
})

watch(() => props.html, () => {
  writeContent()
})
</script>
