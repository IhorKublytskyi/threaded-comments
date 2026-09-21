<script setup>
import { computed, inject } from 'vue'
import { getAttachmentUrl, isImage } from '@/utils/attachment'

const props = defineProps({
  path: { type: String, default: null },
})

const openLightbox = inject('openLightbox')
const openText = inject('openText')

const url = computed(() => getAttachmentUrl(props.path))
const asImage = computed(() => isImage(props.path))

function onError(e) {
  e.target.style.display = 'none'
}

async function openFile() {
  try {
    const res = await fetch(url.value)
    if (!res.ok) throw new Error(`Status ${res.status}`)
    const text = await res.text()
    openText({ name: props.path.split(/[/\\]/).pop(), content: text })
  } catch {
    openText({ name: 'Error', content: 'Failed to load file.' })
  }
}
</script>

<template>
  <div v-if="path" class="mt-2">
    <img
      v-if="asImage"
      :src="url"
      alt="attachment"
      class="attachment-thumb"
      @click="openLightbox(url)"
      @error="onError"
    />
    <a v-else href="#" class="attachment-file" @click.prevent="openFile">
      <span class="icon is-small"><i class="fas fa-file-lines"></i></span>
      <span class="ml-1">attachment.txt</span>
    </a>
  </div>
</template>

<style scoped>
.attachment-thumb {
  max-width: 320px;
  max-height: 240px;
  border-radius: 4px;
  cursor: pointer;
  transition: transform 0.15s ease;
}
.attachment-thumb:hover {
  transform: scale(1.03);
}
</style>
