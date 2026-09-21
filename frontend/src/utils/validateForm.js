export function validateFields(form) {
    const errors = {}

    if (!form.username) {
        errors.username = 'Username is required.'
    } else if (!/^[a-zA-Z0-9]+$/.test(form.username)) {
        errors.username = 'Username can only contain letters and numbers.'
    } else if (form.username.length > 64) {
        errors.username = 'Username must not exceed 64 characters.'
    }

    if (!form.email) {
        errors.email = 'Email is required.'
    } else if (form.email.length > 256) {
        errors.email = 'Email must not exceed 256 characters.'
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
        errors.email = 'Please enter a valid email address.'
    }

    if (form.homePageUrl) {
        if (form.homePageUrl.length > 256) {
            errors.homePageUrl = 'Home page URL must not exceed 256 characters.'
        } else {
            try {
                new URL(form.homePageUrl)
            } catch (e) {
                errors.homePageUrl = 'Please enter a valid URL (e.g., https://example.com).'
            }
        }
    }

    if (!form.body) {
        errors.body = 'Comment text is required.'
    } else if (form.body.length > 1024) {
        errors.body = 'Comment text must not exceed 1024 characters.'
    }

    return errors
}