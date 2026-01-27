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
import { ServicesService } from "../../../services/services.service";
import { CategoriesService } from "../../../services/categories.service";
import { BenefitsService } from "../../../services/benefits.service";
import { ToastService } from "../../../services/toast.service";
import type { Service } from "../../../models/service.model";
import type { Category } from "../../../models/category.model";
import type { Benefit } from "../../../models/benefit.model";
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
  styleUrl: "./services.component.css",
})
export class ServicesComponent implements OnInit {
  private fb = inject(FormBuilder);
  private servicesService = inject(ServicesService);
  private categoriesService = inject(CategoriesService);
  private benefitsService = inject(BenefitsService);
  private toastService = inject(ToastService);

  services = signal<Service[]>([]);
  categories = signal<Category[]>([]);
  benefits = signal<Benefit[]>([]);
  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  confirmId = signal<string | null>(null);
  editingId = signal<string | null>(null);
  isCreating = signal(false);

  form = this.fb.group({
    name: ["", [Validators.required, Validators.maxLength(100)]],
    description: ["", [Validators.required, Validators.maxLength(500)]],
    image: [""], // Keep for existing data, but not used for new uploads
    imageFile: [null as File | null, Validators.required],
    longDescription: ["", Validators.maxLength(2000)],
    duration: ["", Validators.maxLength(50)],
    price: [0, [Validators.required, Validators.min(0)]],
    currency: ["BRL", Validators.required],
    categoryId: ["", Validators.required],
    benefitIds: [[] as string[]],
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
    this.categoriesService.list().subscribe({
      next: (data) => this.categories.set(data),
      error: () => {},
    });
    this.benefitsService.list().subscribe({
      next: (data) => this.benefits.set(data),
      error: () => {},
    });
  }

  startCreate() {
    this.editingId.set(null);
    this.isCreating.set(true);
    this.form.reset({
      currency: "BRL",
      price: 0,
    });
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
      duration: item.duration,
      price: item.price,
      currency: item.currency,
      categoryId: item.category?.id || "",
      benefitIds: item.benefits?.map(b => b.id).filter((id): id is string => Boolean(id)) || [],
    });
    // Make image file optional when editing (image already exists)
    this.form.get('imageFile')?.clearValidators();
    this.form.get('imageFile')?.updateValueAndValidity();
  }

  cancelEdit() {
    this.editingId.set(null);
    this.isCreating.set(false);
    this.form.reset({
      currency: "BRL",
      price: 0,
    });
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
    formData.append('Price', (value.price || 0).toString());
    formData.append('Currency', value.currency || 'BRL');
    
    // Add optional fields
    if (value.longDescription) {
      formData.append('LongDescription', value.longDescription);
    }
    if (value.duration) {
      formData.append('Duration', value.duration);
    }
    
    // Add category (required) - send as lowercase 'category' parameter (matching backend)
    const selectedCategory = this.categories().find(c => c.id === value.categoryId);
    if (!selectedCategory || !selectedCategory.id) {
      this.error.set("Categoria selecionada não encontrada. Por favor, recarregue a página.");
      this.toastService.showError("Categoria selecionada não encontrada");
      return;
    }
    formData.append('category', JSON.stringify({ 
      Id: selectedCategory.id, 
      Name: selectedCategory.name 
    }));
    
    // Add benefits - send as lowercase 'benefits' parameter (matching backend)
    const selectedBenefits = (value.benefitIds || []).filter(Boolean);
    const benefitsArray = selectedBenefits.map(id => {
      const benefit = this.benefits().find(b => b.id === id);
      if (benefit) {
        return { 
          Id: id, 
          Title: benefit.title || "", 
          Description: benefit.description || "" 
        };
      }
      return null;
    }).filter((b): b is { Id: string; Title: string; Description: string } => b !== null);
    
    formData.append('benefits', JSON.stringify(benefitsArray));
    
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
        this.saving.set(false);
        this.cancelEdit();
        this.loadAll();
        const action = this.editingId() ? 'atualizado' : 'criado';
        this.toastService.showSuccess(`Serviço ${action} com sucesso!`);
      },
      error: () => {
        this.saving.set(false);
        this.error.set("Erro ao salvar serviço");
        this.toastService.showError("Erro ao salvar serviço");
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
      error: () => {
        this.error.set("Erro ao excluir serviço");
        this.toastService.showError("Erro ao excluir serviço");
      },
    });
  }

  onBenefitsChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const selectedIds = Array.from(select.selectedOptions, option => option.value);
    this.form.get('benefitIds')?.setValue(selectedIds);
  }
}

