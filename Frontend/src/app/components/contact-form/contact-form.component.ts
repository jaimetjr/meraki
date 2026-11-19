import { Component, DestroyRef, OnInit, inject, signal, viewChild } from "@angular/core"
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms"
import { takeUntilDestroyed } from "@angular/core/rxjs-interop"
import { finalize } from "rxjs/operators"
import { ContactService } from "../../services/contact.service"
import { ServicesService } from "../../services/services.service"
import { ErrorService } from "../../services/error.service"
import { LoggingService } from "../../services/logging.service"
import { TurnstileWidgetComponent } from "../turnstile-widget/turnstile-widget.component"
import { environment } from "../../../environments/environment"
import type { ContactRequest } from "../../models/contact.model"

const OTHER_SERVICE = "Outro"

@Component({
  selector: "app-contact-form",
  standalone: true,
  imports: [ReactiveFormsModule, TurnstileWidgetComponent],
  templateUrl: "./contact-form.component.html",
  styleUrl: "./contact-form.component.css",
})
export class ContactFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder)
  private readonly contactService = inject(ContactService)
  private readonly servicesService = inject(ServicesService)
  private readonly errorService = inject(ErrorService)
  private readonly loggingService = inject(LoggingService)
  private readonly destroyRef = inject(DestroyRef)

  readonly siteKey = environment.turnstileSiteKey
  private readonly captcha = viewChild<{ reset(): void }>("captcha")
  private readonly captchaToken = signal<string | null>(null)

  isSubmitting = signal(false)
  isSubmitted = signal(false)
  captchaMissing = signal(false)

  /** The clinic's active services (from the API) plus "Outro". */
  serviceOptions = signal<string[]>([OTHER_SERVICE])

  contactForm = this.fb.group({
    name: ["", Validators.required],
    email: ["", [Validators.required, Validators.email]],
    phone: ["", Validators.required],
    service: ["", Validators.required],
    message: [""],
    consent: [false, Validators.requiredTrue],
  })

  ngOnInit(): void {
    this.servicesService
      .list()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (services) => this.serviceOptions.set([...services.map((s) => s.name), OTHER_SERVICE]),
        error: (error) => this.loggingService.error("Could not load services for the contact form", error),
      })
  }

  onCaptchaToken(token: string | null): void {
    this.captchaToken.set(token)
    if (token) this.captchaMissing.set(false)
  }

  onSubmit(): void {
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched()
      return
    }
    const captchaToken = this.captchaToken()
    if (!captchaToken) {
      this.captchaMissing.set(true)
      return
    }

    const value = this.contactForm.getRawValue()
    const payload: ContactRequest = {
      name: value.name!,
      email: value.email!,
      phone: value.phone!,
      service: value.service!,
      message: value.message || undefined,
      consent: value.consent === true,
      captchaToken,
    }

    this.isSubmitting.set(true)
    this.contactService
      .send(payload)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSubmitting.set(false)
          // Turnstile tokens are single-use.
          this.captcha()?.reset()
        }),
      )
      .subscribe({
        next: (response) => {
          this.loggingService.info("Contact form submitted successfully", response)
          this.isSubmitted.set(true)
          this.contactForm.reset()
          this.errorService.setError({
            message: response.message || "Mensagem enviada com sucesso! Retornaremos em breve.",
            severity: "success",
            code: "CONTACT_FORM_SUCCESS",
          })
        },
        error: (error) => {
          this.loggingService.error("Failed to submit contact form", error)
          this.errorService.setError({
            message: error?.userMessage || "Erro ao enviar mensagem. Por favor, tente novamente mais tarde.",
            severity: "error",
            code: "CONTACT_FORM_ERROR",
          })
        },
      })
  }

  resetForm(): void {
    this.isSubmitted.set(false)
    this.captchaMissing.set(false)
    this.contactForm.reset()
    this.errorService.clearError()
  }
}
