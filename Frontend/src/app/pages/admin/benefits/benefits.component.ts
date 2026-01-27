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
import { BenefitsService } from "../../../services/benefits.service";
import { ToastService } from "../../../services/toast.service";
import type { Benefit } from "../../../models/benefit.model";

@Component({
  selector: "app-admin-benefits",
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
  templateUrl: "./benefits.component.html",
  styleUrl: "./benefits.component.css",
})
export class BenefitsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private benefitsService = inject(BenefitsService);
  private toastService = inject(ToastService);

  benefits = signal<Benefit[]>([]);
  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  confirmId = signal<string | null>(null);
  editingId = signal<string | null>(null);
  isCreating = signal(false);
  searchTerm = signal("");

  form = this.fb.group({
    title: ["", [Validators.required, Validators.maxLength(200)]],
    description: ["", Validators.maxLength(1000)],
  });

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll() {
    this.loading.set(true);
    this.error.set(null);
    this.benefitsService.list().subscribe({
      next: (data) => this.benefits.set(data),
      error: () => this.error.set("Não foi possível carregar benefícios"),
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
    this.benefitsService.search(term).subscribe({
      next: (data) => this.benefits.set(data),
      error: () => this.error.set("Erro na busca"),
      complete: () => this.loading.set(false),
    });
  }

  startCreate() {
    this.editingId.set(null);
    this.isCreating.set(true);
    this.form.reset();
  }

  startEdit(item: Benefit) {
    this.editingId.set(item.id || null);
    this.isCreating.set(false);
    this.form.patchValue({ title: item.title, description: item.description });
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
    const payload: Benefit = {
      title: this.form.value.title!,
      description: this.form.value.description!,
    };
    this.saving.set(true);
    const request = this.editingId()
      ? this.benefitsService.update(this.editingId()!, payload)
      : this.benefitsService.create(payload);

    (request as Observable<unknown>).subscribe({
      next: () => {
        this.saving.set(false);
        this.cancelEdit();
        this.loadAll();
        const action = this.editingId() ? 'atualizado' : 'criado';
        this.toastService.showSuccess(`Benefício ${action} com sucesso!`);
      },
      error: () => {
        this.saving.set(false);
        this.error.set("Erro ao salvar benefício");
        this.toastService.showError("Erro ao salvar benefício");
      },
    });
  }

  confirmDelete(id: string) {
    this.confirmId.set(id);
  }

  delete(id: string) {
    this.benefitsService.delete(id).subscribe({
      next: () => {
        this.confirmId.set(null);
        this.loadAll();
        this.toastService.showSuccess("Benefício excluído com sucesso!");
      },
      error: () => {
        this.error.set("Erro ao excluir benefício");
        this.toastService.showError("Erro ao excluir benefício");
      },
    });
  }
}

