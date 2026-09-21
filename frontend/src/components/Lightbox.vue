<script setup>
import { watch, onBeforeUnmount } from 'vue'

const props = defineProps({
  src: { type: String, default: null },
})
const emit = defineEmits(['close'])

function onKey(e) {
  if (e.key === 'Escape') emit('close')
}

watch(
  () => props.src,
  (val) => {
    if (val) window.addEventListener('keydown', onKey)
    else window.removeEventListener('keydown', onKey)
  },
)

onBeforeUnmount(() => window.removeEventListener('keydown', onKey))
</script>

<template>
  <Transition name="lb-fade">
    <div v-if="src" class="lightbox" @click.self="emit('close')">
      <button class="lightbox-close" @click="emit('close')" aria-label="close">×</button>
      <img :src="src" alt="preview" class="lightbox-img" />
    </div>
  </Transition>
</template>

<style scoped>
.lightbox {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.85);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
  cursor: zoom-out;
}
.lightbox-img {
  max-width: 90vw;
  max-height: 90vh;
  border-radius: 6px;
  cursor: default;
}
.lightbox-close {
  position: absolute;
  top: 1rem;
  right: 1.25rem;
  font-size: 2rem;
  color: #fff;
  background: none;
  border: none;
  cursor: pointer;
  line-height: 1;
}
.lb-fade-enter-active,
.lb-fade-leave-active {
  transition: opacity 0.2s ease;
}
.lb-fade-enter-from,
.lb-fade-leave-to {
  opacity: 0;
}
</style>
