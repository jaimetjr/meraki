import { Component, inject, signal, DestroyRef } from "@angular/core"
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms"
import { takeUntilDestroyed } from "@angular/core/rxjs-interop"
import { ContactService } from "../../services/contact.service"
import { ErrorService } from "../../services/error.service"
import { LoggingService } from "../../services/logging.service"
import type { ContactRequest } from "../../models/contact.model"
import { catchError, finalize } from "rxjs/operators"
import { throwError } from "rxjs"

@Component({
  selector: "app-contact-form",
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: "./contact-form.component.html",
  styleUrl: "./contact-form.component.css",
})
export class ContactFormComponent {
  private readonly fb = inject(FormBuilder)
  private readonly contactService = inject(ContactService)
  private readonly errorService = inject(ErrorService)
  private readonly loggingService = inject(LoggingService)
  private readonly destroyRef = inject(DestroyRef)

  isSubmitting = signal(false)
  isSubmitted = signal(false)

  services = [
    { value: "acupuntura", label: "Acupuntura" },
    { value: "fisioterapia", label: "Fisioterapia" },
    { value: "massoterapia", label: "Massoterapia" },
    { value: "psicoterapia", label: "Psicoterapia" },
    { value: "nutricao", label: "Nutrição" },
    { value: "outro", label: "Outro" },
  ]

  contactForm = this.fb.group({
    name: ["", Validators.required],
    email: ["", [Validators.required, Validators.email]],
    phone: ["", Validators.required],
    service: ["", Validators.required],
    message: [""],
  })

  onSubmit(): void {
    // Mark all fields as touched to show validation errors
    if (this.contactForm.invalid) {
      Object.keys(this.contactForm.controls).forEach(key => {
        this.contactForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.isSubmitting.set(true);

    const formValue = this.contactForm.value;
    const payload: ContactRequest = {
      name: formValue.name!,
      email: formValue.email!,
      phone: formValue.phone!,
      service: formValue.service!,
      message: formValue.message || undefined,
    };

    this.contactService
      .send(payload)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        catchError((error) => {
          this.loggingService.error("Failed to submit contact form", error);
          const errorMessage = error?.userMessage || 
            "Erro ao enviar mensagem. Por favor, tente novamente mais tarde.";
          this.errorService.setError({
            message: errorMessage,
            severity: "error",
            code: "CONTACT_FORM_ERROR",
          });
          return throwError(() => error);
        }),
        finalize(() => {
          this.isSubmitting.set(false);
        })
      )
      .subscribe({
        next: (response) => {
          this.loggingService.info("Contact form submitted successfully", response);
          this.isSubmitted.set(true);
          this.contactForm.reset();
          this.errorService.clearError();
          // Show success message
          this.errorService.setError({
            message: response.message || "Mensagem enviada com sucesso! Retornaremos em breve.",
            severity: "success",
            code: "CONTACT_FORM_SUCCESS",
          });
        },
      });
  }

  resetForm(): void {
    this.isSubmitted.set(false);
    this.contactForm.reset();
    this.errorService.clearError();
  }
}
