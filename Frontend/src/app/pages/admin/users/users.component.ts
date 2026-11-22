import { Component, OnInit, signal, inject, computed } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import type { Observable } from "rxjs";
import { AdminPanelComponent } from "../../../components/admin/admin-panel/admin-panel.component";
import { AdminEmptyComponent } from "../../../components/admin/admin-empty/admin-empty.component";
import { AdminLoadingComponent } from "../../../components/admin/admin-loading/admin-loading.component";
import { AdminConfirmDialogComponent } from "../../../components/admin/admin-confirm-dialog/admin-confirm-dialog.component";
import { AdminFormActionsComponent } from "../../../components/admin/admin-form-actions/admin-form-actions.component";
import { AdminFormModalComponent } from "../../../components/admin/admin-form-modal/admin-form-modal.component";
import { UsersService } from "../../../services/users.service";
import { ToastService } from "../../../services/toast.service";
import { AuthService } from "../../../services/auth.service";
import type { User, CreateUserDto, UpdateUserDto } from "../../../models/user.model";
import { UserRole } from "../../../models/user.model";

/** Must match the API (CreateUserValidator.PasswordMinLength). */
const PASSWORD_MIN_LENGTH = 8;

@Component({
  selector: "app-admin-users",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AdminPanelComponent,
    AdminEmptyComponent,
    AdminLoadingComponent,
    AdminConfirmDialogComponent,
    AdminFormActionsComponent,
    AdminFormModalComponent,
  ],
  templateUrl: "./users.component.html",
  styleUrl: "./users.component.css",
})
export class UsersComponent implements OnInit {
  private fb = inject(FormBuilder);
  private usersService = inject(UsersService);
  private toastService = inject(ToastService);
  private currentUserId = inject(AuthService).getCurrentUser()?.userId ?? null;

  users = signal<User[]>([]);
  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  confirmId = signal<string | null>(null);
  editingId = signal<string | null>(null);
  isCreating = signal(false);
  userRoles = Object.values(UserRole);
  
  // Expose UserRole to template
  readonly UserRole = UserRole;

  /** The API refuses to let you change your own role or deactivate/delete yourself, so the page doesn't offer it. */
  editingSelf = computed(() => this.editingId() !== null && this.isSelf(this.editingId()));

  isSelf(id: string | null | undefined): boolean {
    return !!id && id === this.currentUserId;
  }

  form = this.fb.group({
    email: ["", [Validators.required, Validators.email]],
    password: ["", [Validators.minLength(PASSWORD_MIN_LENGTH)]],
    firstName: [""],
    lastName: [""],
    role: [UserRole.Admin as UserRole, Validators.required],
    isActive: [true],
  });

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll() {
    this.loading.set(true);
    this.error.set(null);
    this.usersService.list().subscribe({
      next: (data) => this.users.set(data),
      error: () => this.error.set("Não foi possível carregar usuários"),
      complete: () => this.loading.set(false),
    });
  }

  /** Disabled controls are left out of form.value, so the update request keeps your current role and status. */
  private lockOwnRoleAndStatus(locked: boolean) {
    for (const name of ["role", "isActive"]) {
      const control = this.form.get(name);
      if (locked) control?.disable({ emitEvent: false });
      else control?.enable({ emitEvent: false });
    }
  }

  startCreate() {
    this.editingId.set(null);
    this.isCreating.set(true);
    this.lockOwnRoleAndStatus(false);
    this.form.reset({
      role: UserRole.Admin,
      isActive: true,
    });
    // Require password for new users
    this.form.get('password')?.setValidators([Validators.required, Validators.minLength(PASSWORD_MIN_LENGTH)]);
    this.form.get('password')?.updateValueAndValidity();
  }

  startEdit(user: User) {
    this.editingId.set(user.id || null);
    this.isCreating.set(false);
    this.lockOwnRoleAndStatus(this.isSelf(user.id));
    this.form.patchValue({
      email: user.email,
      firstName: user.firstName || "",
      lastName: user.lastName || "",
      role: user.role,
      isActive: user.isActive,
      password: "", // Clear password field
    });
    // Password is optional when editing
    this.form.get('password')?.clearValidators();
    this.form.get('password')?.setValidators([Validators.minLength(PASSWORD_MIN_LENGTH)]);
    this.form.get('password')?.updateValueAndValidity();
  }

  cancelEdit() {
    this.editingId.set(null);
    this.isCreating.set(false);
    this.lockOwnRoleAndStatus(false);
    this.form.reset({
      role: UserRole.Admin,
      isActive: true,
    });
    // Reset password validation
    this.form.get('password')?.setValidators([Validators.required, Validators.minLength(PASSWORD_MIN_LENGTH)]);
    this.form.get('password')?.updateValueAndValidity();
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    
    const value = this.form.value;
    
    if (this.isCreating()) {
      const roleValue = value.role || UserRole.Admin;
      // Convert enum to string value for API
      const roleString = roleValue.toString();
      const payload = {
        email: value.email || "",
        password: value.password || "",
        firstName: value.firstName || undefined,
        lastName: value.lastName || undefined,
        role: roleString,
      };
      
      // Remove undefined values
      Object.keys(payload).forEach(key => {
        const typedKey = key as keyof typeof payload;
        if (payload[typedKey] === undefined) {
          delete payload[typedKey];
        }
      });

      this.saving.set(true);
      (this.usersService.create(payload as CreateUserDto) as Observable<unknown>).subscribe({
        next: () => {
          this.saving.set(false);
          this.cancelEdit();
          this.loadAll();
          this.toastService.showSuccess("Usuário criado com sucesso!");
        },
        error: (err) => {
          this.saving.set(false);
          const errorMsg = err?.userMessage || err?.message || "Erro ao criar usuário";
          this.error.set(errorMsg);
          this.toastService.showError(errorMsg);
        },
      });
    } else {
      const roleValue = value.role;
      // Convert enum to string value for API if provided
      const roleString = roleValue ? roleValue.toString() : undefined;
      const payload = {
        email: value.email || undefined,
        firstName: value.firstName || undefined,
        lastName: value.lastName || undefined,
        role: roleString,
        isActive: value.isActive ?? undefined,
      };

      // Remove undefined values
      Object.keys(payload).forEach(key => {
        const typedKey = key as keyof typeof payload;
        if (payload[typedKey] === undefined) {
          delete payload[typedKey];
        }
      });

      this.saving.set(true);
      (this.usersService.update(this.editingId()!, payload as UpdateUserDto) as Observable<unknown>).subscribe({
        next: () => {
          this.saving.set(false);
          // If password was provided, update it separately
          if (value.password && value.password.length >= PASSWORD_MIN_LENGTH) {
            this.usersService.updatePassword(this.editingId()!, value.password).subscribe({
              next: () => {
                this.cancelEdit();
                this.loadAll();
                this.toastService.showSuccess("Usuário atualizado com sucesso!");
              },
              error: () => {
                this.error.set("Usuário atualizado, mas falha ao atualizar senha");
                this.saving.set(false);
                this.toastService.showWarning("Usuário atualizado, mas falha ao atualizar senha");
              },
            });
          } else {
            this.cancelEdit();
            this.loadAll();
            this.toastService.showSuccess("Usuário atualizado com sucesso!");
          }
        },
        error: (err) => {
          this.saving.set(false);
          const errorMsg = err?.userMessage || err?.message || "Erro ao atualizar usuário";
          this.error.set(errorMsg);
          this.toastService.showError(errorMsg);
        },
      });
    }
  }

  confirmDelete(id: string) {
    this.confirmId.set(id);
  }

  delete(id: string) {
    this.usersService.delete(id).subscribe({
      next: () => {
        this.confirmId.set(null);
        this.loadAll();
        this.toastService.showSuccess("Usuário excluído com sucesso!");
      },
      error: () => {
        this.error.set("Erro ao excluir usuário");
        this.toastService.showError("Erro ao excluir usuário");
      },
    });
  }

  getRoleLabel(role: UserRole): string {
    return role === UserRole.SuperAdmin ? "Super Admin" : "Admin";
  }
}

