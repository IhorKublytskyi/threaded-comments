<script setup>
import Comment from './Comment.vue'

const props = defineProps({
  comments: Object,
})

const emit = defineEmits(['reply'])
</script>

<template>
  <div class="is-flex is-flex-direction-column mt-4">
    <Comment
      v-for="comment in comments.items"
      :key="comment.id"
      :comment="comment"
      @reply="emit('reply', $event)"
    ></Comment>
  </div>
</template>
