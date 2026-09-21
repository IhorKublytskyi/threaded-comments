<script setup>
import { validateFields } from '@/utils/validateForm'
import { ref, watch } from 'vue'
import { apiUrl } from '@/utils/api.js'

const props = defineProps({
  replyTo: { type: Object, default: null },
})

const form = ref({
  email: '',
  username: '',
  homePageUrl: '',
  body: '',
})

watch(
  () => props.replyTo,
  (target, previous) => {
    if (!target?.username) return

    let body = form.value.body
    if (previous?.username) {
      const prevPrefix = `@${previous.username} `
      if (body.startsWith(prevPrefix)) {
        body = body.slice(prevPrefix.length)
      }
    }

    form.value.body = `@${target.username} ` + body
  },
)

// validation
const fieldErrors = ref({})

function clearFieldError(field) {
  if (fieldErrors.value[field]) {
    delete fieldErrors.value[field]
  }
}

const emit = defineEmits(['created'])

const submitting = ref(false)
const loading = ref(false)

const file = ref(null)
const fileError = ref('')

const bodyRef = ref(null)
const isPreviewMode = ref(false)
const tagError = ref('')

const captcha = ref(null)
const captchaAnswer = ref('')
const showCaptcha = ref(false)
const captchaError = ref('')

async function onFileChange(event) {
  fileError.value = ''
  file.value = null

  const selectedFile = event.target.files[0]
  if (!selectedFile) return

  const isText = selectedFile.type == 'text/plain'
  const isImage = ['image/jpeg', 'image/png', 'image/gif'].includes(selectedFile.type)

  if (!isText && !isImage) {
    fileError.value = 'Only .txt, .jpg, .png, and .gif files are allowed.'
    event.target.value = ''
    return
  }

  if (isText) {
    if (selectedFile.size > 100 * 1024) {
      fileError.value = 'The text file must not exceed 100 KB'
      event.target.value = ''
      return
    }
    file.value = selectedFile
  }

  if (isImage) {
    try {
      file.value = await resizeImage(selectedFile, 320, 240)
    } catch (e) {
      fileError.value = 'Error processing the image'
      event.target.value = ''
    }
  }
}

function resizeImage(originalFile, maxWidth, maxHeight) {
  return new Promise((resolve, reject) => {
    const img = new Image()
    const url = URL.createObjectURL(originalFile)

    img.onload = () => {
      URL.revokeObjectURL(url)
      let { width, height } = img

      if (width <= maxWidth && height <= maxHeight) {
        return resolve(originalFile)
      }

      if (width > maxWidth) {
        height = Math.round((height * maxWidth) / width)
        width = maxWidth
      }
      if (height > maxHeight) {
        width = Math.round((width * maxHeight) / height)
        height = maxHeight
      }

      const canvas = document.createElement('canvas')
      canvas.width = width
      canvas.height = height
      const ctx = canvas.getContext('2d')
      ctx.drawImage(img, 0, 0, width, height)

      canvas.toBlob((blob) => {
        if (!blob) return reject(new Error('Canvas error'))
        resolve(new File([blob], originalFile.name, { type: originalFile.type }))
      }, originalFile.type)
    }

    img.onerror = reject
    img.src = url
  })
}

function insertTag(tag) {
  const textarea = bodyRef.value
  if (!textarea) return

  const start = textarea.selectionStart
  const end = textarea.selectionEnd

  const selectedText = form.value.body.substring(start, end)
  const openTag = tag === 'a' ? '<a href="" title="">' : `<${tag}>`
  const closeTag = `</${tag}>`

  form.value.body =
    form.value.body.substring(0, start) +
    openTag +
    selectedText +
    closeTag +
    form.value.body.substring(end)

  tagError.value = ''
}

function togglePreview() {
  if (isPreviewMode.value) {
    isPreviewMode.value = false
    tagError.value = ''
    return
  }

  tagError.value = ''

  if (!form.value.body) {
    isPreviewMode.value = true
    return
  }

  const parser = new DOMParser()
  const doc = parser.parseFromString(`<root>${form.value.body}</root>`, 'application/xml')

  if (doc.querySelector('parsererror')) {
    tagError.value = 'Invalid or unclosed HTML tags found.'
    return
  }

  const allowedTags = ['root', 'a', 'code', 'i', 'strong']
  const allowedAttrs = { a: ['href', 'title'] }

  for (const el of doc.querySelectorAll('*')) {
    const tagName = el.tagName.toLowerCase()

    if (!allowedTags.includes(tagName)) {
      tagError.value = `The HTML tag <${tagName}> is not allowed.`
      return
    }

    const allowed = allowedAttrs[tagName] || []
    for (const attr of el.attributes) {
      if (!allowed.includes(attr.name.toLowerCase())) {
        tagError.value = `The attribute '${attr.name}' is not allowed in <${tagName}> tag.`
        return
      }
    }

    if (tagName === 'a') {
      const href = (el.getAttribute('href') || '').trim()
      if (href && !/^(https?:|mailto:|\/|#|\.\.?\/)/i.test(href)) {
        tagError.value = 'Malicious URL detected.'
        return
      }
    }
  }

  isPreviewMode.value = true
}

function clearTagError() {
  tagError.value = ''
}

function openCaptchaModal() {
  const validationErrors = validateFields(form.value)

  if (Object.keys(validationErrors).length > 0) {
    fieldErrors.value = validationErrors
    return
  }

  captchaError.value = ''
  showCaptcha.value = true
  loadCaptcha()
}

async function loadCaptcha() {
  captchaAnswer.value = ''
  loading.value = true
  try {
    const res = await fetch(apiUrl('/captcha'))
    if (res.ok) {
      captcha.value = await res.json()
    }
  } catch (e) {
    console.log(e.message)
  } finally {
    loading.value = false
  }
}

async function onFormSubmit() {
  submitting.value = true

  try {
    const fd = new FormData()

    const replyPrefix = props.replyTo?.username ? `@${props.replyTo.username} ` : ''
    if (props.replyTo?.id && replyPrefix && form.value.body.startsWith(replyPrefix)) {
      fd.append('ParentCommentId', props.replyTo.id)
    }

    fd.append('Username', form.value.username)
    fd.append('Email', form.value.email)
    if (form.value.homePageUrl) {
      fd.append('HomePageUrl', form.value.homePageUrl)
    }
    fd.append('Body', form.value.body)

    fd.append('CaptchaAnswer.Input', captchaAnswer.value)
    if (captcha.value && captcha.value.token) {
      fd.append('CaptchaAnswer.Token', captcha.value.token)
    }

    if (file.value) {
      fd.append('File', file.value)
    }

    const res = await fetch(apiUrl('/comments'), {
      method: 'POST',
      body: fd,
    })

    if (res.ok) {
      const replyPrefix = props.replyTo?.username ? `@${props.replyTo.username} ` : ''
      const hasParent = props.replyTo?.id && replyPrefix && form.value.body.startsWith(replyPrefix)
      const parentId = hasParent ? props.replyTo.id : null

      form.value = { email: '', username: '', homePageUrl: '', body: '' }
      file.value = null
      captchaAnswer.value = ''
      showCaptcha.value = false

      emit('created', parentId)
    } else {
      const problem = await res.json().catch(() => null)

      if (problem?.errors) {
        const map = {
          username: 'username',
          email: 'email',
          homepageurl: 'homePageUrl',
          body: 'body',
          'captchaanswer.input': 'captchaAnswer',
        }

        for (const [field, messages] of Object.entries(problem.errors)) {
          const key = map[field] ?? field
          const text = Array.isArray(messages) ? messages.join(' ') : messages
          if (key === 'captchaAnswer') {
            captchaError.value = text
          } else {
            fieldErrors.value[key] = text
          }
        }

        if (!problem.errors['captchaanswer.input']) {
          captchaError.value = 'Please fix the highlighted fields.'
        }
      } else {
        captchaError.value = problem?.detail || 'Something went wrong. Try again.'
      }
      loadCaptcha()
    }
  } catch (e) {
    console.error('Network error:', e)
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <form class="box mt-4" @submit.prevent="openCaptchaModal" novalidate>
    <div class="field">
      <p class="control has-icons-left has-icons-right">
        <input
          v-model="form.email"
          class="input"
          :class="{ 'is-danger': fieldErrors.email }"
          type="text"
          placeholder="Email"
          @input="clearFieldError('email')"
        />
        <span class="icon is-small is-left">
          <i class="fas fa-envelope"></i>
        </span>
        <span class="icon is-small is-right">
          <i class="fas fa-check"></i>
        </span>
      </p>
      <p v-if="fieldErrors.email" class="help is-danger mt-1">{{ fieldErrors.email }}</p>
    </div>
    <div class="field">
      <p class="control has-icons-left has-icons-right">
        <input
          v-model="form.username"
          class="input"
          :class="{ 'is-danger': fieldErrors.username }"
          type="text"
          placeholder="Username"
          @input="clearFieldError('username')"
        />
        <span class="icon is-small is-left">
          <i class="fa-regular fa-circle-user"></i>
        </span>
        <span class="icon is-small is-right">
          <i class="fas fa-check"></i>
        </span>
      </p>
      <p v-if="fieldErrors.username" class="help is-danger mt-1">{{ fieldErrors.username }}</p>
    </div>
    <div class="field">
      <p class="control has-icons-left">
        <input
          v-model="form.homePageUrl"
          class="input"
          :class="{ 'is-danger': fieldErrors.homePageUrl }"
          type="text"
          placeholder="Home page URL"
          @input="clearFieldError('homePageUrl')"
        />
        <span class="icon is-small is-left">
          <i class="fa-solid fa-link"></i>
        </span>
      </p>
      <p v-if="fieldErrors.homePageUrl" class="help is-danger mt-1">
        {{ fieldErrors.homePageUrl }}
      </p>
    </div>
    <div class="buttons mt-4 are-small">
      <button class="button" type="button" @click="insertTag('a')" :disabled="isPreviewMode">
        a
      </button>
      <button class="button" type="button" @click="insertTag('code')" :disabled="isPreviewMode">
        code
      </button>
      <button class="button" type="button" @click="insertTag('i')" :disabled="isPreviewMode">
        i
      </button>
      <button class="button" type="button" @click="insertTag('strong')" :disabled="isPreviewMode">
        strong
      </button>
    </div>
    <div class="field">
      <div class="control">
        <textarea
          v-if="!isPreviewMode"
          ref="bodyRef"
          v-model="form.body"
          class="textarea is-small"
          :class="{ 'is-danger': fieldErrors.body }"
          placeholder="Add a comment..."
          style="height: 150px"
          @input="(clearTagError(), clearFieldError('body'))"
        ></textarea>

        <div
          v-else
          class="textarea is-small content"
          style="min-height: 150px; overflow-y: auto"
          v-html="form.body"
        ></div>
      </div>
      <p v-if="tagError" class="help is-danger mt-1">{{ tagError }}</p>
      <p v-if="fieldErrors.body" class="help is-danger mt-1">{{ fieldErrors.body }}</p>
    </div>
    <div class="is-flex is-justify-content-space-between is-align-items-center mt-4">
      <div class="buttons mb-0">
        <div class="file is-small">
          <label class="file-label">
            <input
              class="file-input"
              type="file"
              accept=".txt, image/jpeg, image/png, image/gif"
              @change="onFileChange"
            />
            <span class="file-cta">
              <span class="file-icon">
                <i class="fas fa-upload"></i>
              </span>
              <span
                class="file-label"
                style="
                  max-width: 150px;
                  overflow: hidden;
                  text-overflow: ellipsis;
                  white-space: nowrap;
                "
              >
                {{ file ? file.name : 'Choose a file…' }}
              </span>
            </span>
          </label>
        </div>
        <p v-if="fileError" class="help is-danger mt-1">{{ fileError }}</p>
      </div>
      <div class="buttons are-small mb-0">
        <button class="button is-small mr-2" type="button" @click="togglePreview">
          {{ isPreviewMode ? 'Edit' : 'Preview' }}
        </button>
        <button class="button is-info" type="submit">Submit</button>
      </div>
    </div>
  </form>

  <!-- Captcha modal -->
  <div class="modal" :class="{ 'is-active': showCaptcha }">
    <div class="modal-background" @click="showCaptcha = false"></div>
    <div class="modal-card">
      <header class="modal-card-head">
        <p class="modal-card-title">Prove you are not a robot</p>
        <button
          class="delete"
          aria-label="close"
          type="button"
          @click="showCaptcha = false"
        ></button>
      </header>

      <section class="modal-card-body">
        <div v-if="loading" class="has-text-centered">Loading...</div>
        <template v-else-if="captcha">
          <div class="is-flex is-justify-content-center has-background-white">
            <img :src="`data:image/png;base64,${captcha.imageBase64}`" alt="captcha" />
          </div>
        </template>

        <input
          v-model="captchaAnswer"
          class="input mt-3"
          :class="{ 'is-danger': captchaError }"
          placeholder="Enter the captcha"
          :disabled="submitting"
          @keyup.enter="onFormSubmit"
          @input="captchaError = ''"
        />
        <p v-if="captchaError" class="help is-danger mt-2">{{ captchaError }}</p>
      </section>

      <footer class="modal-card-foot">
        <div class="buttons are-small">
          <button class="button" type="button" @click="showCaptcha = false">Cancel</button>
          <button
            class="button is-info"
            type="button"
            :disabled="!captchaAnswer"
            @click="onFormSubmit"
          >
            {{ submitting ? 'Checking...' : 'Submit' }}
          </button>
        </div>
      </footer>
    </div>
  </div>
</template>
