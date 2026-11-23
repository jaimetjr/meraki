import type { MockedObject } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { CategoriesComponent } from './categories.component';
import { CategoriesService } from '../../../services/categories.service';
import { ToastService } from '../../../services/toast.service';

describe('CategoriesComponent (admin)', () => {
  let fixture: ComponentFixture<CategoriesComponent>;
  let component: CategoriesComponent;
  let categories: MockedObject<Pick<CategoriesService, 'list' | 'create' | 'update' | 'delete'>>;
  let toast: MockedObject<Pick<ToastService, 'showSuccess' | 'showError' | 'showWarning'>>;

  beforeEach(async () => {
    categories = {
      list: vi.fn().mockName('CategoriesService.list'),
      create: vi.fn().mockName('CategoriesService.create'),
      update: vi.fn().mockName('CategoriesService.update'),
      delete: vi.fn().mockName('CategoriesService.delete')
    };
    categories.list.mockReturnValue(of([{ id: 'c1', name: 'Terapia' }]));
    toast = {
      showSuccess: vi.fn().mockName('ToastService.showSuccess'),
      showError: vi.fn().mockName('ToastService.showError'),
      showWarning: vi.fn().mockName('ToastService.showWarning')
    };

    await TestBed.configureTestingModule({
      imports: [CategoriesComponent],
      providers: [
        { provide: CategoriesService, useValue: categories },
        { provide: ToastService, useValue: toast },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CategoriesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // The error interceptor puts the API's reason (or a generic text) in userMessage.
  const rejected = (userMessage: string) => throwError(() => ({ status: 409, userMessage }));

  it("tells the admin why a category can't be deleted", () => {
    const reason = 'Esta categoria é usada por 1 serviço(s). Mova-os para outra categoria antes de excluí-la.';
    categories.delete.mockReturnValue(rejected(reason));

    component.delete('c1');

    expect(toast.showError).toHaveBeenCalledWith(reason);
    expect(component.error()).toBe(reason);
  });

  it("tells the admin why a category can't be saved", () => {
    const reason = "'Nome' deve ser menor ou igual a 100 caracteres.";
    categories.create.mockReturnValue(rejected(reason));
    component.startCreate();
    component.form.patchValue({ name: 'Nova' });

    component.submit();

    expect(toast.showError).toHaveBeenCalledWith(reason);
  });
});
