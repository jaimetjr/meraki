import { Component, OnInit, signal, inject, ChangeDetectionStrategy } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import type { Observable } from "rxjs";
import { AdminPanelComponent } from "../../../components/admin/admin-panel/admin-panel.component";
import { AdminEmptyComponent } from "../../../components/admin/admin-empty/admin-empty.component";
import { AdminLoadingComponent } from "../../../components/admin/admin-loading/admin-loading.component";
import { AdminConfirmDialogComponent } from "../../../components/admin/admin-confirm-dialog/admin-confirm-dialog.component";
import { AdminFormActionsComponent } from "../../../components/admin/admin-form-actions/admin-form-actions.component";
import { AdminFormModalComponent } from "../../../components/admin/admin-form-modal/admin-form-modal.component";
import { TherapistsService } from "../../../services/therapists.service";
import { ServicesService } from "../../../services/services.service";
import { ConsultorioMEService, type ConsultorioMEProfessional } from "../../../services/consultoriome.service";
import { ToastService } from "../../../services/toast.service";
import type { Therapist } from "../../../models/therapist.model";
import type { Service } from "../../../models/service.model";
import { urlValidator } from "../../../validators/custom.validators";
import { environment } from "../../../../environments/environment";

@Component({
  selector: "app-admin-therapists",
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AdminPanelComponent,
    AdminEmptyComponent,
    AdminLoadingComponent,
    AdminConfirmDialogComponent,
    AdminFormActionsComponent,
    AdminFormModalComponent,
  ],
  templateUrl: "./therapists.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: "./therapists.component.css",
})
export class TherapistsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private therapistsService = inject(TherapistsService);
  private servicesService = inject(ServicesService);
  private consultorioMEService = inject(ConsultorioMEService);
  private toastService = inject(ToastService);

  therapists = signal<Therapist[]>([]);
  /** The clinic's services, shown as checkboxes ("Serviços que atende"). */
  services = signal<Service[]>([]);
  professionals = signal<ConsultorioMEProfessional[]>([]);
  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  confirmId = signal<string | null>(null);
  editingId = signal<string | null>(null);
  isCreating = signal(false);
  /** While off, the ConsultorioME field is hidden but a therapist's saved proId is still sent back. */
  readonly consultorioMEEnabled = environment.consultorioME;

  form = this.fb.group({
    name: ["", [Validators.required, Validators.maxLength(100)]],
    bio: ["", [Validators.required, Validators.maxLength(500)]],
    image: [""], // Keep for existing data, but not used for new uploads
    imageFile: [null as File | null, Validators.required],
    experience: ["", Validators.required],
    education: ["", Validators.required],
    proId: [""],
    serviceIds: [[] as string[]],
  });

  ngOnInit(): void {
    this.loadAll();
    if (this.consultorioMEEnabled) {
      this.loadProfessionals();
    }
  }

  loadAll() {
    this.loading.set(true);
    this.error.set(null);
    this.therapistsService.list().subscribe({
      next: (data) => this.therapists.set(data),
      error: () => this.error.set("Não foi possível carregar terapeutas"),
      complete: () => this.loading.set(false),
    });
    this.servicesService.list().subscribe({
      next: (data) => this.services.set(data),
      error: () => {},
    });
  }

  loadProfessionals() {
    this.consultorioMEService.getProfessionals().subscribe({
      next: (data) => this.professionals.set(data),
      error: () => {
        // Silently fail - professionals are optional
        console.warn("Failed to load professionals from ConsultorioME");
      },
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
      proId: item.proId || "",
      serviceIds: (item.services ?? []).map((s) => s.id),
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
    
    // Add ProId if provided
    if (value.proId) {
      formData.append('proId', value.proId);
    }
    
    // The API replaces the therapist's services with exactly these ids and reads nothing else,
    // so send the ticked ids as they are: never look them up in the checkbox list, which may
    // not have loaded (an empty list here would remove every service).
    const serviceIds = (value.serviceIds || []).filter(Boolean);
    formData.append('services', JSON.stringify(serviceIds.map((id) => ({ id }))));
    
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
        // Before cancelEdit(), which clears editingId.
        const action = this.editingId() ? 'atualizado' : 'criado';
        this.saving.set(false);
        this.cancelEdit();
        this.loadAll();
        this.toastService.showSuccess(`Terapeuta ${action} com sucesso!`);
      },
      error: (err) => {
        this.saving.set(false);
        const message = err?.userMessage || "Erro ao salvar terapeuta";
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
    this.therapistsService.delete(id).subscribe({
      next: () => {
        this.confirmId.set(null);
        this.loadAll();
        this.toastService.showSuccess("Terapeuta excluído com sucesso!");
      },
      error: (err) => {
        const message = err?.userMessage || "Erro ao excluir terapeuta";
        this.error.set(message);
        this.toastService.showError(message);
      },
    });
  }

  servicesLabel(item: Therapist): string {
    return (item.services || []).map((s) => s.name).join(", ") || "-";
  }

  offersService(serviceId: string | undefined): boolean {
    return !!serviceId && (this.form.value.serviceIds ?? []).includes(serviceId);
  }

  toggleService(serviceId: string | undefined, offered: boolean) {
    if (!serviceId) return;
    const current = (this.form.value.serviceIds ?? []).filter((id) => id !== serviceId);
    this.form.patchValue({ serviceIds: offered ? [...current, serviceId] : current });
  }
}

