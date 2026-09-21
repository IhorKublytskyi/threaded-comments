<script setup>
import { inject, watch, computed, ref } from 'vue'
import { formatDate } from '@/utils/formatDate.js'
import CommentAttachment from './CommentAttachment.vue'

const props = defineProps({
  comment: { type: Object, required: true },
  depth: { type: Number, default: 0 },
})

const emit = defineEmits(['reply'])

const localReplyCount = ref(props.comment.repliesCount ?? 0)
const reloadReplies = inject('reloadReplies', ref(null))

watch(reloadReplies, (val) => {
  if (val && val.id === props.comment.id) {
    localReplyCount.value += 1
    loadReplies(true)
  }
})

const replies = ref([])
const loading = ref(false)
const expanded = ref(false)
const error = ref('')

const API = 'http://localhost:5046'
const hasReplies = computed(() => localReplyCount.value > 0)

async function loadReplies(force = false) {
  if (!hasReplies.value || loading.value) return
  if (replies.value.length && !force) {
    expanded.value = !expanded.value
    return
  }
  loading.value = true
  error.value = ''
  try {
    const res = await fetch(`${API}/comments/${props.comment.id}/replies`)
    if (!res.ok) throw new Error(`Status ${res.status}`)
    const data = await res.json()
    replies.value = Array.isArray(data) ? data : data.items
    expanded.value = true
  } catch (e) {
    error.value = 'Failed to load replies.'
  } finally {
    loading.value = false
  }
}
function toggleReplies() {
  loadReplies(false)
}
</script>

<template>
  <article class="media" :style="{ marginLeft: Math.min(depth, 6) * 24 + 'px' }">
    <div class="media-content">
      <div class="content">
        <p class="mb-2">
          <a
            v-if="comment.homePageUrl"
            :href="comment.homePageUrl"
            target="_blank"
            rel="nofollow noopener ugc"
          >
            <strong>{{ comment.username }}</strong>
          </a>
          <strong v-else>{{ comment.username }}</strong>
          <small class="has-text-grey ml-2">{{ comment.email }}</small>
          <small class="has-text-grey ml-2">· {{ formatDate(comment.createdAt) }}</small>
        </p>
        <p v-html="comment.body"></p>
        <CommentAttachment :path="comment.attachmentPath" />
      </div>
      <nav class="level is-mobile">
        <div class="level-left">
          <a
            class="level-item is-size-7"
            @click="emit('reply', { id: comment.id, username: comment.username })"
          >
            <span class="icon is-small"><i class="fas fa-reply"></i></span>
          </a>

          <a
            v-if="hasReplies"
            class="level-item is-size-7"
            :disabled="loading"
            @click="toggleReplies"
          >
            <span class="icon is-small">
              <i class="fas" :class="expanded ? 'fa-chevron-up' : 'fa-chevron-down'"></i>
            </span>
            <span class="ml-1">
              {{ loading ? '…' : `${localReplyCount} replies` }}
            </span>
          </a>
        </div>
      </nav>
      <p v-if="error" class="help is-danger">{{ error }}</p>

      <template v-if="expanded">
        <Comment
          v-for="child in replies"
          :key="child.id"
          :comment="child"
          :depth="depth + 1"
          @reply="emit('reply', $event)"
        />
      </template>
    </div>
  </article>
</template>
