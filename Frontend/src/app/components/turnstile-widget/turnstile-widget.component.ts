import { AfterViewInit, Component, ElementRef, OnDestroy, input, output, viewChild, ChangeDetectionStrategy } from "@angular/core"

interface TurnstileApi {
  render(element: HTMLElement, options: Record<string, unknown>): string
  reset(widgetId?: string): void
  remove(widgetId: string): void
}

declare global {
  interface Window {
    turnstile?: TurnstileApi
  }
}

const SCRIPT_URL = "https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit"
let scriptLoading: Promise<TurnstileApi> | null = null

function loadTurnstile(): Promise<TurnstileApi> {
  if (window.turnstile) return Promise.resolve(window.turnstile)
  scriptLoading ??= new Promise<TurnstileApi>((resolve, reject) => {
    const script = document.createElement("script")
    script.src = SCRIPT_URL
    script.async = true
    script.defer = true
    script.onload = () => (window.turnstile ? resolve(window.turnstile) : reject(new Error("Turnstile did not load")))
    script.onerror = () => {
      scriptLoading = null
      reject(new Error("Could not load Turnstile"))
    }
    document.head.appendChild(script)
  })
  return scriptLoading
}

/**
 * Cloudflare Turnstile CAPTCHA. Emits the token once the visitor passes (null when it expires
 * or fails). Tokens are single-use: call reset() after every submission attempt.
 */
@Component({
  selector: "app-turnstile-widget",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  template: `<div #container></div>`,
})
export class TurnstileWidgetComponent implements AfterViewInit, OnDestroy {
  siteKey = input.required<string>()
  token = output<string | null>()

  private container = viewChild.required<ElementRef<HTMLElement>>("container")
  private widgetId?: string
  private destroyed = false

  async ngAfterViewInit(): Promise<void> {
    try {
      const turnstile = await loadTurnstile()
      if (this.destroyed) return
      this.widgetId = turnstile.render(this.container().nativeElement, {
        sitekey: this.siteKey(),
        language: "pt-br",
        callback: (token: string) => this.token.emit(token),
        "expired-callback": () => this.token.emit(null),
        "error-callback": () => this.token.emit(null),
      })
    } catch {
      this.token.emit(null)
    }
  }

  reset(): void {
    if (this.widgetId) window.turnstile?.reset(this.widgetId)
    this.token.emit(null)
  }

  ngOnDestroy(): void {
    this.destroyed = true
    if (this.widgetId) window.turnstile?.remove(this.widgetId)
  }
}
