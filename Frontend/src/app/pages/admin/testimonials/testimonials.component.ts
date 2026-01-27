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
import { TestimonialsService } from "../../../services/testimonials.service";
import { ToastService } from "../../../services/toast.service";
import type { Testimonial } from "../../../models/testimonial.model";

@Component({
  selector: "app-admin-testimonials",
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
  templateUrl: "./testimonials.component.html",
  styleUrl: "./testimonials.component.css",
})
export class TestimonialsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private testimonialsService = inject(TestimonialsService);
  private toastService = inject(ToastService);

  testimonials = signal<Testimonial[]>([]);
  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  confirmId = signal<string | null>(null);
  editingId = signal<string | null>(null);
  isCreating = signal(false);

  form = this.fb.group({
    authorName: ["", [Validators.required, Validators.maxLength(100)]],
    authorAvatarUrl: [""], // Keep for existing data, but not used for new uploads
    imageFile: [null as File | null, Validators.required],
    authorBadge: [""],
    rating: [5, [Validators.required, Validators.min(1), Validators.max(5)]],
    content: ["", [Validators.required, Validators.maxLength(1000)]],
  });

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll() {
    this.loading.set(true);
    this.error.set(null);
    this.testimonialsService.list().subscribe({
      next: (data) => this.testimonials.set(data),
      error: () => this.error.set("Não foi possível carregar depoimentos"),
      complete: () => this.loading.set(false),
    });
  }

  startCreate() {
    this.editingId.set(null);
    this.isCreating.set(true);
    this.form.reset({ rating: 5 });
    // Require image file for new testimonials
    this.form.get('imageFile')?.setValidators(Validators.required);
    this.form.get('imageFile')?.updateValueAndValidity();
  }

  startEdit(item: Testimonial) {
    this.editingId.set(item.id || null);
    this.isCreating.set(false);
    this.form.patchValue({
      authorName: item.authorName,
      authorAvatarUrl: item.authorAvatarUrl,
      imageFile: null,
      authorBadge: item.authorBadge || "",
      rating: item.rating,
      content: item.content,
    });
    // Make image file optional when editing (image already exists)
    this.form.get('imageFile')?.clearValidators();
    this.form.get('imageFile')?.updateValueAndValidity();
  }

  cancelEdit() {
    this.editingId.set(null);
    this.isCreating.set(false);
    this.form.reset({ rating: 5 });
    // Reset image file validation
    this.form.get('imageFile')?.setValidators(Validators.required);
    this.form.get('imageFile')?.updateValueAndValidity();
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    
    const value = this.form.value;
    const formData = new FormData();
    
    // Add required fields
    formData.append('authorName', value.authorName || '');
    formData.append('rating', String(value.rating || 5));
    formData.append('content', value.content || '');
    
    if (value.authorBadge) {
      formData.append('authorBadge', value.authorBadge);
    }
    
    // Always send AuthorAvatarUrl (PascalCase as expected by backend) - as file if provided, or existing URL when editing
    if (value.imageFile) {
      formData.append('image', value.imageFile);
    } else if (this.editingId() && value.authorAvatarUrl) {
      // When editing without new image, send existing URL as string
      formData.append('image', value.authorAvatarUrl);
    } else {
      // When creating without file (shouldn't happen due to validation, but backend requires the field)
      formData.append('image', '');
    }

    this.saving.set(true);
    const request = this.editingId()
      ? this.testimonialsService.update(this.editingId()!, formData)
      : this.testimonialsService.create(formData);

    (request as Observable<unknown>).subscribe({
      next: () => {
        this.saving.set(false);
        this.cancelEdit();
        this.loadAll();
        const action = this.editingId() ? 'atualizado' : 'criado';
        this.toastService.showSuccess(`Depoimento ${action} com sucesso!`);
      },
      error: () => {
        this.saving.set(false);
        this.error.set("Erro ao salvar depoimento");
        this.toastService.showError("Erro ao salvar depoimento");
      },
    });
  }

  onImageFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.form.patchValue({ imageFile: input.files[0] });
      this.form.get('imageFile')?.updateValueAndValidity();
    } else {
      this.form.patchValue({ imageFile: null });
      this.form.get('imageFile')?.updateValueAndValidity();
    }
  }

  confirmDelete(id: string) {
    this.confirmId.set(id);
  }

  delete(id: string) {
    this.testimonialsService.delete(id).subscribe({
      next: () => {
        this.confirmId.set(null);
        this.loadAll();
        this.toastService.showSuccess("Depoimento excluído com sucesso!");
      },
      error: () => {
        this.error.set("Erro ao excluir depoimento");
        this.toastService.showError("Erro ao excluir depoimento");
      },
    });
  }
}

