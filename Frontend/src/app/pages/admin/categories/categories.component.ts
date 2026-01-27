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
import { CategoriesService } from "../../../services/categories.service";
import { ToastService } from "../../../services/toast.service";
import type { Category } from "../../../models/category.model";

@Component({
  selector: "app-admin-categories",
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
  templateUrl: "./categories.component.html",
  styleUrl: "./categories.component.css",
})
export class CategoriesComponent implements OnInit {
  private fb = inject(FormBuilder);
  private categoriesService = inject(CategoriesService);
  private toastService = inject(ToastService);

  categories = signal<Category[]>([]);
  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  confirmId = signal<string | null>(null);
  editingId = signal<string | null>(null);
  isCreating = signal(false);

  form = this.fb.group({
    name: ["", [Validators.required, Validators.maxLength(100)]],
  });

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll() {
    this.loading.set(true);
    this.error.set(null);
    this.categoriesService.list().subscribe({
      next: (data) => this.categories.set(data),
      error: () => this.error.set("Não foi possível carregar categorias"),
      complete: () => this.loading.set(false),
    });
  }

  startCreate() {
    this.editingId.set(null);
    this.isCreating.set(true);
    this.form.reset();
  }

  startEdit(item: Category) {
    this.editingId.set(item.id || null);
    this.isCreating.set(false);
    this.form.patchValue({ name: item.name });
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
    const payload: Category = { name: this.form.value.name! };
    this.saving.set(true);
    const request = this.editingId()
      ? this.categoriesService.update(this.editingId()!, payload)
      : this.categoriesService.create(payload);

    (request as Observable<unknown>).subscribe({
      next: () => {
        this.saving.set(false);
        this.cancelEdit();
        this.loadAll();
        const action = this.editingId() ? 'atualizada' : 'criada';
        this.toastService.showSuccess(`Categoria ${action} com sucesso!`);
      },
      error: () => {
        this.saving.set(false);
        this.error.set("Erro ao salvar categoria");
        this.toastService.showError("Erro ao salvar categoria");
      },
    });
  }

  confirmDelete(id: string) {
    this.confirmId.set(id);
  }

  delete(id: string) {
    this.categoriesService.delete(id).subscribe({
      next: () => {
        this.confirmId.set(null);
        this.loadAll();
        this.toastService.showSuccess("Categoria excluída com sucesso!");
      },
      error: () => {
        this.error.set("Erro ao excluir categoria");
        this.toastService.showError("Erro ao excluir categoria");
      },
    });
  }
}

