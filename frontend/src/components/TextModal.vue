<script setup>
defineProps({
  visible: Boolean,
  name: String,
  content: String,
})
const emit = defineEmits(['close'])
</script>

<template>
  <div v-if="visible" class="modal is-active">
    <div class="modal-background" @click="emit('close')"></div>
    <div class="modal-card">
      <header class="modal-card-head">
        <p class="modal-card-title">{{ name }}</p>
        <button class="delete" @click="emit('close')"></button>
      </header>
      <section class="modal-card-body">
        <pre class="text-content">{{ content }}</pre>
      </section>
    </div>
  </div>
</template>

<style scoped>
.text-content {
  white-space: pre-wrap;
  word-break: break-word;
  max-height: 60vh;
  overflow: auto;
  font-size: 0.85rem;
}
</style>
