<script setup>
import { onMounted, ref, computed, provide } from 'vue'
import CommentsList from './components/CommentsList.vue'
import PaginationBar from './components/PaginationBar.vue'
import PostCommentForm from './components/PostCommentForm.vue'
import Lightbox from './components/Lightbox.vue'
import TextModal from './components/TextModal.vue'

const textFile = ref(null)
provide('openText', (payload) => {
  textFile.value = payload
})
const lightboxSrc = ref(null)
provide('openLightbox', (url) => {
  lightboxSrc.value = url
})

const replyTo = ref(null)
const reloadTarget = ref(null)
provide('reloadReplies', reloadTarget)

function onCommentCreated(parentId) {
  replyTo.value = null
  if (parentId != null) {
    reloadTarget.value = { id: parentId, ts: Date.now() }
  } else {
    fetchData()
  }
}
function handleReply(target) {
  replyTo.value = target

  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const items = ref({ items: [], count: 0 })
const page = ref(1)
const pageSize = 25
const sortBy = ref('CreatedAt')

const sortOptions = [
  { label: 'Date', value: 'CreatedAt' },
  { label: 'Username', value: 'Username' },
  { label: 'Email', value: 'Email' },
]

const currentSortLabel = computed(
  () => sortOptions.find((o) => o.value === sortBy.value)?.label || 'Date',
)

function setSort(value) {
  if (sortBy.value === value) {
    isDesc.value = !isDesc.value
  } else {
    sortBy.value = value
    isDesc.value = true
  }
  isDropdownOpen.value = false
  page.value = 1
  fetchData()
}

const isDropdownOpen = ref(false)

const isDesc = ref(true)
const totalPages = computed(() => Math.ceil((items.value.count || 0) / pageSize))

function buildGetCommentsUrl(url) {
  return url + `?Parameters=(${page.value},${pageSize},${sortBy.value},${isDesc.value})`
}

async function fetchData() {
  const url = buildGetCommentsUrl('http://localhost:5046/comments')

  console.log(url)

  try {
    const response = await fetch(url, { method: 'GET' })

    if (!response.ok) {
      throw new Error(`Response status: ${response.status}`)
    }

    const result = await response.json()

    items.value = result
  } catch (error) {
    console.error(error.message)
  }
}

onMounted(() => {
  fetchData()
})
</script>

<template>
  <section>
    <div class="container is-max-desktop is-flex is-justify-content-center mt-4">
      <h1 class="title is-3">Comments section</h1>
    </div>
    <div class="container is-max-desktop mt-5">
      <PostCommentForm :reply-to="replyTo" @created="onCommentCreated"></PostCommentForm>
    </div>
  </section>
  <section>
    <div class="container is-max-desktop mt-4">
      <div class="box">
        <div class="is-flex is-justify-content-space-between is-align-items-center mt-0">
          <h1 class="subtitle is-6 mb-0">Comments ({{ items.count }})</h1>
          <div class="dropdown is-left" :class="{ 'is-active': isDropdownOpen }">
            <div class="dropdown-trigger">
              <button
                class="button"
                aria-haspopup="true"
                aria-controls="dropdown-menu-sortby"
                @click="isDropdownOpen = !isDropdownOpen"
              >
                <span>{{ currentSortLabel }} · {{ isDesc ? 'DESC' : 'ASC' }}</span>
                <span class="icon is-small"
                  ><i class="fas fa-angle-down" aria-hidden="true"></i
                ></span>
              </button>
            </div>
            <div class="dropdown-menu" id="dropdown-menu-sortby">
              <div class="dropdown-content">
                <a
                  v-for="opt in sortOptions"
                  :key="opt.value"
                  href="#"
                  class="dropdown-item"
                  :class="{ 'is-active': sortBy === opt.value }"
                  @click.prevent="setSort(opt.value)"
                  >{{ opt.label }}</a
                >
              </div>
            </div>
          </div>
        </div>

        <!-- Comments -->
        <CommentsList :comments="items" @reply="handleReply"></CommentsList>
        <PaginationBar
          :current-page="page"
          :total-pages="totalPages"
          @change="
            (p) => {
              page = p
              fetchData()
            }
          "
        ></PaginationBar>
      </div>
    </div>
  </section>
  <Lightbox :src="lightboxSrc" @close="lightboxSrc = null" />
  <TextModal
    :visible="!!textFile"
    :name="textFile?.name"
    :content="textFile?.content"
    @close="textFile = null"
  />
</template>

<style scoped></style>
