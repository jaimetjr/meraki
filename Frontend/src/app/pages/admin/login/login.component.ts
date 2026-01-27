import { Component, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { AuthService } from "../../../services/auth.service";
import type { LoginRequest } from "../../../models/auth.model";

@Component({
  selector: "app-admin-login",
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: "./login.component.html",
  styleUrl: "./login.component.css",
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  errorMessage = signal<string | null>(null);

  form = this.fb.group({
    email: ["", [Validators.required, Validators.email]],
    password: ["", [Validators.required, Validators.minLength(6)]],
  });

  constructor() {
    if (this.authService.isAuthenticated()) {
      this.router.navigateByUrl("/admin");
    }
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.value as LoginRequest;
    this.loading.set(true);
    this.errorMessage.set(null);

    this.authService.login(payload).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigateByUrl("/admin");
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(
          err?.userMessage ||
          err?.message ||
          "Não foi possível realizar o login. Verifique suas credenciais."
        );
      },
    });
  }
}

