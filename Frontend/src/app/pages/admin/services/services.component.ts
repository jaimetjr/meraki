import { Component, OnInit, signal, inject, ChangeDetectionStrategy } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import type { Observable } from "rxjs";
import { AdminPanelComponent } from "../../../components/admin/admin-panel/admin-panel.component";
import { AdminEmptyComponent } from "../../../components/admin/admin-empty/admin-empty.component";
import { AdminLoadingComponent } from "../../../components/admin/admin-loading/admin-loading.component";
import { AdminConfirmDialogComponent } from "../../../components/admin/admin-confirm-dialog/admin-confirm-dialog.component";
import { AdminFormActionsComponent } from "../../../components/admin/admin-form-actions/admin-form-actions.component";
import { AdminFormModalComponent } from "../../../components/admin/admin-form-modal/admin-form-modal.component";
import { ServicesService } from "../../../services/services.service";
import { ToastService } from "../../../services/toast.service";
import type { Service } from "../../../models/service.model";
import { urlValidator } from "../../../validators/custom.validators";

@Component({
  selector: "app-admin-services",
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
  templateUrl: "./services.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: "./services.component.css",
})
export class ServicesComponent implements OnInit {
  private fb = inject(FormBuilder);
  private servicesService = inject(ServicesService);
  private toastService = inject(ToastService);

  services = signal<Service[]>([]);
  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  confirmId = signal<string | null>(null);
  editingId = signal<string | null>(null);
  isCreating = signal(false);

  form = this.fb.group({
    name: ["", [Validators.required, Validators.maxLength(100)]],
    description: ["", [Validators.required, Validators.maxLength(300)]],
    image: [""], // Keep for existing data, but not used for new uploads
    imageFile: [null as File | null, Validators.required],
    longDescription: ["", Validators.maxLength(1000)],
    duration: ["", Validators.maxLength(50)],
    // Empty = no published price ("sob consulta").
    price: [null as number | null, Validators.min(0)],
    // One benefit per line.
    benefits: [""],
  });

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll() {
    this.loading.set(true);
    this.error.set(null);
    this.servicesService.list().subscribe({
      next: (data) => this.services.set(data),
      error: () => this.error.set("Não foi possível carregar serviços"),
      complete: () => this.loading.set(false),
    });
  }

  startCreate() {
    this.editingId.set(null);
    this.isCreating.set(true);
    this.form.reset();
    // Require image file for new services
    this.form.get('imageFile')?.setValidators(Validators.required);
    this.form.get('imageFile')?.updateValueAndValidity();
  }

  startEdit(item: Service) {
    this.editingId.set(item.id || null);
    this.isCreating.set(false);
    this.form.patchValue({
      name: item.name,
      description: item.description,
      image: item.image,
      imageFile: null,
      longDescription: item.longDescription,
      duration: item.duration ?? "",
      price: item.price,
      benefits: (item.benefits ?? []).join("\n"),
    });
    // Make image file optional when editing (image already exists)
    this.form.get('imageFile')?.clearValidators();
    this.form.get('imageFile')?.updateValueAndValidity();
  }

  cancelEdit() {
    this.editingId.set(null);
    this.isCreating.set(false);
    this.form.reset();
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
    formData.append('Name', value.name || '');
    formData.append('Description', value.description || '');
    // Use existing image URL if editing and no new file uploaded, otherwise empty (file will be uploaded)
    formData.append('Image', value.imageFile ? '' : (value.image || ''));
    if (value.price !== null && value.price !== undefined) {
      formData.append('Price', value.price.toString());
      formData.append('Currency', 'BRL');
    }
    
    // Add optional fields
    if (value.longDescription) {
      formData.append('LongDescription', value.longDescription);
    }
    if (value.duration) {
      formData.append('Duration', value.duration);
    }
    
    // The API replaces the service's benefits with exactly this list: one per typed line.
    formData.append('benefits', JSON.stringify(ServicesComponent.benefitLines(value.benefits)));
    
    // Add image file (required)
    if (value.imageFile) {
      formData.append('image', value.imageFile);
    }

    this.saving.set(true);
    const request = this.editingId()
      ? this.servicesService.update(this.editingId()!, formData)
      : this.servicesService.create(formData);

    (request as Observable<unknown>).subscribe({
      next: () => {
        // Before cancelEdit(), which clears editingId.
        const action = this.editingId() ? 'atualizado' : 'criado';
        this.saving.set(false);
        this.cancelEdit();
        this.loadAll();
        this.toastService.showSuccess(`Serviço ${action} com sucesso!`);
      },
      error: (err) => {
        this.saving.set(false);
        const message = err?.userMessage || "Erro ao salvar serviço";
        this.error.set(message);
        this.toastService.showError(message);
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
    this.servicesService.delete(id).subscribe({
      next: () => {
        this.confirmId.set(null);
        this.loadAll();
        this.toastService.showSuccess("Serviço excluído com sucesso!");
      },
      error: (err) => {
        const message = err?.userMessage || "Erro ao excluir serviço";
        this.error.set(message);
        this.toastService.showError(message);
      },
    });
  }

  /** The benefits textarea as a list: one per line, trimmed, without empty lines. */
  static benefitLines(text: string | null | undefined): string[] {
    return (text ?? "").split("\n").map((line) => line.trim()).filter(Boolean);
  }
}

