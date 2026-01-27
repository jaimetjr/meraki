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
import { TherapistsService } from "../../../services/therapists.service";
import { SpecialtiesService } from "../../../services/specialties.service";
import { ToastService } from "../../../services/toast.service";
import type { Therapist } from "../../../models/therapist.model";
import type { Specialty } from "../../../models/specialty.model";
import { urlValidator } from "../../../validators/custom.validators";

@Component({
  selector: "app-admin-therapists",
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
  templateUrl: "./therapists.component.html",
  styleUrl: "./therapists.component.css",
})
export class TherapistsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private therapistsService = inject(TherapistsService);
  private specialtiesService = inject(SpecialtiesService);
  private toastService = inject(ToastService);

  therapists = signal<Therapist[]>([]);
  specialties = signal<Specialty[]>([]);
  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  confirmId = signal<string | null>(null);
  editingId = signal<string | null>(null);
  isCreating = signal(false);

  form = this.fb.group({
    name: ["", [Validators.required, Validators.maxLength(100)]],
    bio: ["", [Validators.required, Validators.maxLength(500)]],
    image: [""], // Keep for existing data, but not used for new uploads
    imageFile: [null as File | null, Validators.required],
    experience: ["", Validators.required],
    education: ["", Validators.required],
    specialtyIds: [[] as string[]],
  });

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll() {
    this.loading.set(true);
    this.error.set(null);
    this.therapistsService.list().subscribe({
      next: (data) => this.therapists.set(data),
      error: () => this.error.set("Não foi possível carregar terapeutas"),
      complete: () => this.loading.set(false),
    });
    this.specialtiesService.list().subscribe({
      next: (data) => this.specialties.set(data),
      error: () => {},
    });
  }

  startCreate() {
    this.editingId.set(null);
    this.isCreating.set(true);
    this.form.reset();
    // Require image file for new therapists
    this.form.get('imageFile')?.setValidators(Validators.required);
    this.form.get('imageFile')?.updateValueAndValidity();
  }

  startEdit(item: Therapist) {
    this.editingId.set(item.id || null);
    this.isCreating.set(false);
    this.form.patchValue({
      name: item.name,
      bio: item.bio,
      image: item.image,
      imageFile: null,
      experience: item.experience,
      education: item.education,
      specialtyIds: item.specialties?.map(s => s.id).filter((id): id is string => Boolean(id)) || [],
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
    
    // Add required fields - use lowercase field names
    formData.append('name', value.name || '');
    formData.append('bio', value.bio || '');
    formData.append('experience', value.experience || '');
    formData.append('education', value.education || '');
    
    // Add specialties - send as specialtiesJson with JSON string
    const selectedSpecialties = (value.specialtyIds || []).filter(Boolean);
    const specialtiesArray = selectedSpecialties.map(id => {
      const specialty = this.specialties().find(s => s.id === id);
      if (specialty) {
        return {
          id: specialty.id,
          name: specialty.name,
          description: specialty.description
        };
      }
      return null;
    }).filter((s): s is { id: string; name: string; description: string } => s !== null);
    
    formData.append('specialties', JSON.stringify(specialtiesArray));
    
    // Add image file if provided
    if (value.imageFile) {
      formData.append('image', value.imageFile);
    }

    this.saving.set(true);
    const request = this.editingId()
      ? this.therapistsService.update(this.editingId()!, formData)
      : this.therapistsService.create(formData);

    (request as Observable<unknown>).subscribe({
      next: () => {
        this.saving.set(false);
        this.cancelEdit();
        this.loadAll();
        const action = this.editingId() ? 'atualizado' : 'criado';
        this.toastService.showSuccess(`Terapeuta ${action} com sucesso!`);
      },
      error: () => {
        this.saving.set(false);
        this.error.set("Erro ao salvar terapeuta");
        this.toastService.showError("Erro ao salvar terapeuta");
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
    this.therapistsService.delete(id).subscribe({
      next: () => {
        this.confirmId.set(null);
        this.loadAll();
        this.toastService.showSuccess("Terapeuta excluído com sucesso!");
      },
      error: () => {
        this.error.set("Erro ao excluir terapeuta");
        this.toastService.showError("Erro ao excluir terapeuta");
      },
    });
  }

  specialtiesLabel(item: Therapist): string {
    return (item.specialties || []).map((s) => s.name).join(", ") || "-";
  }

  onSpecialtiesChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const selectedIds = Array.from(select.selectedOptions, option => option.value);
    this.form.get('specialtyIds')?.setValue(selectedIds);
  }
}

