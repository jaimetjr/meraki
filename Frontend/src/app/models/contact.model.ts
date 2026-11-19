/** What the public contact form sends to POST /api/contact. */
export interface ContactRequest {
  name: string
  email: string
  phone: string
  service: string
  message?: string
  /** The visitor agreed that the clinic stores their data to answer them (LGPD). */
  consent: boolean
  /** Cloudflare Turnstile token. */
  captchaToken: string
}

export interface ContactResponse {
  success: boolean
  message: string
}

/** A stored message as listed in the admin inbox. */
export interface ContactMessage {
  id: string
  name: string
  email: string
  phone: string
  service: string
  message?: string | null
  isRead: boolean
  createdAt: string
}
