<script setup>
import { computed } from 'vue'

const emit = defineEmits(['change'])

const props = defineProps({
  currentPage: { type: Number, default: 1 },
  totalPages: { type: Number, default: 1 },
})

const pages = computed(() => {
  const total = props.totalPages
  const current = props.currentPage
  const set = new Set([1, total])
  for (let p = current - 2; p <= current + 2; p++) {
    if (p >= 1 && p <= total) set.add(p)
  }

  return [...set].sort((a, b) => a - b)
})

function goTo(page) {
  if (page < 1 || page > props.totalPages || page === props.currentPage) return
  emit('change', page)
}
</script>

<template>
  <div v-if="totalPages > 1" class="mt-4">
    <nav class="pagination is-small" role="navigation" aria-label="pagination">
      <button
        href="#"
        class="pagination-previous"
        :disabled="currentPage === 1"
        @click.prevent="goTo(currentPage - 1)"
        >Previous</button
      >
      <button
        href="#"
        class="pagination-next"
        :disabled="currentPage === totalPages"
        @click.prevent="goTo(currentPage + 1)"
        >Next</button
      >
      <ul class="pagination-list">
        <template v-for="(p, i) in pages" :key="p">
          <li v-if="i > 0 && p - pages[i - 1] > 1">
            <span class="pagination-ellipsis">&hellip;</span>
          </li>
          <li>
            <a
              href="#"
              class="pagination-link"
              :class="{ 'is-current': p === currentPage }"
              :aria-label="`Goto page ${p}`"
              @click.prevent="goTo(p)"
              >{{ p }}</a
            >
          </li>
        </template>
      </ul>
    </nav>
  </div>
</template>
