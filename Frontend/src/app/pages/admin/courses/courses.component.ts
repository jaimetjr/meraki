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
import { CoursesService } from "../../../services/courses.service";
import { ToastService } from "../../../services/toast.service";
import type { Course } from "../../../models/course.model";
import { CourseStatus, CourseStatusDisplayNames, getCourseStatusDisplayName } from "../../../enums/course-status.enum";
import { CourseType } from "../../../enums/course-type.enum";
import { Modality } from "../../../enums/modality.enum";
import { urlValidator, dateRangeValidator } from "../../../validators/custom.validators";

@Component({
  selector: "app-admin-courses",
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
  templateUrl: "./courses.component.html",
  styleUrl: "./courses.component.css",
})
export class CoursesComponent implements OnInit {
  private fb = inject(FormBuilder);
  private coursesService = inject(CoursesService);
  private toastService = inject(ToastService);

  courses = signal<Course[]>([]);
  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  confirmId = signal<string | null>(null);
  editingId = signal<string | null>(null);
  isCreating = signal(false);

  // Enum values for template
  CourseStatus = CourseStatus;
  CourseStatusDisplayNames = CourseStatusDisplayNames;
  CourseType = CourseType;
  Modality = Modality;
  
  // Helper methods
  getStatusDisplayName = getCourseStatusDisplayName;
  getStatusOptions() {
    return Object.values(CourseStatus);
  }
  getTypeOptions() {
    return Object.values(CourseType);
  }
  getModalityOptions() {
    return Object.values(Modality);
  }

  form = this.fb.group({
    title: ["", [Validators.required, Validators.maxLength(200)]],
    description: ["", [Validators.required, Validators.maxLength(1000)]],
    image: [""], // Keep for existing data, but not used for new uploads
    imageFile: [null as File | null, Validators.required],
    instructor: ["", Validators.required],
    type: [CourseType.Curso, Validators.required],
    status: [CourseStatus.Disponivel, Validators.required],
    startDate: [""],
    endDate: [""],
    modality: [Modality.Presencial],
    price: [0, Validators.min(0)],
    currency: ["BRL"],
    link: ["", urlValidator()],
  }, { validators: dateRangeValidator() });

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll() {
    this.loading.set(true);
    this.error.set(null);
    this.coursesService.list().subscribe({
      next: (data) => this.courses.set(data),
      error: () => this.error.set("Não foi possível carregar cursos"),
      complete: () => this.loading.set(false),
    });
  }

  startCreate() {
    this.editingId.set(null);
    this.isCreating.set(true);
    this.form.reset({
      status: CourseStatus.Disponivel,
      currency: "BRL",
      price: 0,
      modality: Modality.Presencial,
      type: CourseType.Curso,
    });
    // Require image file for new courses
    this.form.get('imageFile')?.setValidators(Validators.required);
    this.form.get('imageFile')?.updateValueAndValidity();
  }

  startEdit(item: Course) {
    this.editingId.set(item.id || null);
    this.isCreating.set(false);
    this.form.patchValue({
      title: item.title,
      description: item.description,
      image: item.image,
      imageFile: null,
      instructor: item.instructor,
      type: item.type as CourseType,
      status: item.status as CourseStatus,
      startDate: item.startDate ? this.formatDateForInput(item.startDate) : "",
      endDate: item.endDate ? this.formatDateForInput(item.endDate) : "",
      modality: item.modality as Modality,
      price: item.price || 0,
      currency: item.currency || "BRL",
      link: item.link || "",
    });
    // Make image file optional when editing (image already exists)
    this.form.get('imageFile')?.clearValidators();
    this.form.get('imageFile')?.updateValueAndValidity();
  }

  formatDateForInput(dateString: string): string {
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  cancelEdit() {
    this.editingId.set(null);
    this.isCreating.set(false);
    this.form.reset({
      status: CourseStatus.Disponivel,
      currency: "BRL",
      price: 0,
      modality: Modality.Presencial,
      type: CourseType.Curso,
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
    
    const formValue = this.form.value;
    const formData = new FormData();
    
    // Add required fields
    formData.append('Title', formValue.title || '');
    formData.append('Description', formValue.description || '');
    // Use existing image URL if editing and no new file uploaded, otherwise empty (file will be uploaded)
    formData.append('Image', formValue.imageFile ? '' : (formValue.image || ''));
    formData.append('Instructor', formValue.instructor || '');
    formData.append('Type', formValue.type || '');
    formData.append('Status', formValue.status || '');
    
    // Add optional date fields
    if (formValue.startDate) {
      const startDate = new Date(formValue.startDate);
      formData.append('StartDate', startDate.toISOString());
    }
    if (formValue.endDate) {
      const endDate = new Date(formValue.endDate);
      formData.append('EndDate', endDate.toISOString());
    }
    
    // Add optional fields
    if (formValue.modality) {
      formData.append('Modality', formValue.modality);
    }
    if (formValue.price !== undefined && formValue.price !== null) {
      formData.append('Price', formValue.price.toString());
    }
    if (formValue.currency) {
      formData.append('Currency', formValue.currency);
    }
    if (formValue.link) {
      formData.append('Link', formValue.link);
    }
    
    // Add image file if provided (required for new, optional for edit)
    if (formValue.imageFile) {
      formData.append('image', formValue.imageFile);
    }
    
    this.saving.set(true);
    const request = this.editingId()
      ? this.coursesService.update(this.editingId()!, formData)
      : this.coursesService.create(formData);

    (request as Observable<unknown>).subscribe({
      next: () => {
        this.saving.set(false);
        this.cancelEdit();
        this.loadAll();
        const action = this.editingId() ? 'atualizado' : 'criado';
        this.toastService.showSuccess(`Curso ${action} com sucesso!`);
      },
      error: () => {
        this.saving.set(false);
        this.error.set("Erro ao salvar curso");
        this.toastService.showError("Erro ao salvar curso");
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
    this.coursesService.delete(id).subscribe({
      next: () => {
        this.confirmId.set(null);
        this.loadAll();
        this.toastService.showSuccess("Curso excluído com sucesso!");
      },
      error: () => {
        this.error.set("Erro ao excluir curso");
        this.toastService.showError("Erro ao excluir curso");
      },
    });
  }
}

