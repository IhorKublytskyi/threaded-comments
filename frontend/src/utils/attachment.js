const API = 'http://localhost:5046'

export function getAttachmentUrl(path) {
    if (!path) return null
    return `${API}/attachments/${path.replace(/\\/g, '/')}`
}

const IMAGE_EXT = ['jpg', 'jpeg', 'png', 'gif']

export function isImage(path) {
    if (!path) return false
    const ext = path.split('.').pop().toLowerCase()
    return IMAGE_EXT.includes(ext)
}