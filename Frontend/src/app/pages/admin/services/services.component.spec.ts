import type { MockedObject } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ServicesComponent } from './services.component';
import { ServicesService } from '../../../services/services.service';
import { CategoriesService } from '../../../services/categories.service';
import { BenefitsService } from '../../../services/benefits.service';
import { ToastService } from '../../../services/toast.service';
import type { Service } from '../../../models/service.model';

describe('ServicesComponent (admin)', () => {
  let fixture: ComponentFixture<ServicesComponent>;
  let component: ServicesComponent;
  let servicesService: MockedObject<Pick<ServicesService, 'list' | 'create' | 'update' | 'delete'>>;

  const category = { id: 'c1', name: 'Terapias' };
  const existing: Service = {
    id: 'sv1', name: 'Reiki', description: 'Desc', longDescription: '', duration: '1h', price: 100, currency: 'BRL',
    category,
    benefits: [{ id: 'b1', title: 'Relaxamento', description: '' }, { id: 'b2', title: 'Sono', description: '' }],
  } as Service;

  beforeEach(async () => {
    servicesService = {
      list: vi.fn().mockName('ServicesService.list'),
      create: vi.fn().mockName('ServicesService.create'),
      update: vi.fn().mockName('ServicesService.update'),
      delete: vi.fn().mockName('ServicesService.delete')
    };
    servicesService.list.mockReturnValue(of([existing]));
    servicesService.update.mockReturnValue(of(void 0));
    const categoriesService = {
      list: vi.fn().mockName('CategoriesService.list')
    };
    categoriesService.list.mockReturnValue(of([category]));
    const benefitsService = {
      list: vi.fn().mockName('BenefitsService.list')
    };
    // The benefits list failed to load, so the dropdown has no options.
    benefitsService.list.mockReturnValue(throwError(() => new Error('network')));

    await TestBed.configureTestingModule({
      imports: [ServicesComponent],
      providers: [
        { provide: ServicesService, useValue: servicesService },
        { provide: CategoriesService, useValue: categoriesService },
        { provide: BenefitsService, useValue: benefitsService },
        { provide: ToastService, useValue: { showSuccess: vi.fn(), showError: vi.fn(), showWarning: vi.fn() } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ServicesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('keeps the service\'s benefits when only the description is edited, even if the benefits list did not load', () => {
    component.startEdit(existing);
    component.form.patchValue({ description: 'Nova descrição' });

    component.submit();

    const sent = servicesService.update.mock.lastCall![1] as FormData;
    const ids = (JSON.parse(sent.get('benefits') as string) as { id: string }[]).map((b) => b.id);
    expect(ids).toEqual(['b1', 'b2']);
  });
});
