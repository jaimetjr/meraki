import { Component, OnInit, signal, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import type { Observable } from "rxjs";
import { AdminPanelComponent } from "../../../components/admin/admin-panel/admin-panel.component";
import { AdminEmptyComponent } from "../../../components/admin/admin-empty/admin-empty.component";
import { AdminLoadingComponent } from "../../../components/admin/admin-loading/admin-loading.component";
import { AdminConfirmDialogComponent } from "../../../components/admin/admin-confirm-dialog/admin-confirm-dialog.component";
import { AdminFormActionsComponent } from "../../../components/admin/admin-form-actions/admin-form-actions.component";
import { AdminFormModalComponent } from "../../../components/admin/admin-form-modal/admin-form-modal.component";
import { SpecialtiesService } from "../../../services/specialties.service";
import { ToastService } from "../../../services/toast.service";
import type { Specialty } from "../../../models/specialty.model";

@Component({
  selector: "app-admin-specialties",
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
  templateUrl: "./specialties.component.html",
  styleUrl: "./specialties.component.css",
})
export class SpecialtiesComponent implements OnInit {
  private fb = inject(FormBuilder);
  private specialtiesService = inject(SpecialtiesService);
  private toastService = inject(ToastService);

  specialties = signal<Specialty[]>([]);
  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  confirmId = signal<string | null>(null);
  editingId = signal<string | null>(null);
  isCreating = signal(false);
  searchTerm = signal("");

  form = this.fb.group({
    name: ["", [Validators.required, Validators.maxLength(100)]],
    description: ["", Validators.maxLength(500)],
  });

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll() {
    this.loading.set(true);
    this.error.set(null);
    this.specialtiesService.list().subscribe({
      next: (data) => this.specialties.set(data),
      error: () => this.error.set("Não foi possível carregar especialidades"),
      complete: () => this.loading.set(false),
    });
  }

  search() {
    const term = this.searchTerm().trim();
    if (!term) {
      this.loadAll();
      return;
    }
    this.loading.set(true);
    this.specialtiesService.getByName(term).subscribe({
      next: (data) => this.specialties.set(data),
      error: () => this.error.set("Erro na busca"),
      complete: () => this.loading.set(false),
    });
  }

  startCreate() {
    this.editingId.set(null);
    this.isCreating.set(true);
    this.form.reset();
  }

  startEdit(item: Specialty) {
    this.editingId.set(item.id || null);
    this.isCreating.set(false);
    this.form.patchValue({ name: item.name, description: item.description });
  }

  cancelEdit() {
    this.editingId.set(null);
    this.isCreating.set(false);
    this.form.reset();
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const payload: Specialty = {
      name: this.form.value.name!,
      description: this.form.value.description!,
    };
    this.saving.set(true);
    const request = this.editingId()
      ? this.specialtiesService.update(this.editingId()!, payload)
      : this.specialtiesService.create(payload);

    (request as Observable<unknown>).subscribe({
      next: () => {
        this.saving.set(false);
        this.cancelEdit();
        this.loadAll();
        const action = this.editingId() ? 'atualizada' : 'criada';
        this.toastService.showSuccess(`Especialidade ${action} com sucesso!`);
      },
      error: () => {
        this.saving.set(false);
        this.error.set("Erro ao salvar especialidade");
        this.toastService.showError("Erro ao salvar especialidade");
      },
    });
  }

  confirmDelete(id: string) {
    this.confirmId.set(id);
  }

  delete(id: string) {
    this.specialtiesService.delete(id).subscribe({
      next: () => {
        this.confirmId.set(null);
        this.loadAll();
        this.toastService.showSuccess("Especialidade excluída com sucesso!");
      },
      error: () => {
        this.error.set("Erro ao excluir especialidade");
        this.toastService.showError("Erro ao excluir especialidade");
      },
    });
  }
}

