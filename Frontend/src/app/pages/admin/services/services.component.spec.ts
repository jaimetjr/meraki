import type { MockedObject } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ServicesComponent } from './services.component';
import { ServicesService } from '../../../services/services.service';
import { ToastService } from '../../../services/toast.service';
import type { Service } from '../../../models/service.model';

describe('ServicesComponent (admin)', () => {
  let fixture: ComponentFixture<ServicesComponent>;
  let component: ServicesComponent;
  let servicesService: MockedObject<Pick<ServicesService, 'list' | 'create' | 'update' | 'delete'>>;
  let toast: MockedObject<Pick<ToastService, 'showSuccess' | 'showError' | 'showWarning'>>;

  const existing: Service = {
    id: 'sv1', name: 'Reiki', description: 'Desc', longDescription: '', duration: '1h', price: 100, currency: 'BRL',
    benefits: ['Relaxamento', 'Sono'], therapists: [],
  };
  const withoutPrice: Service = {
    id: 'sv2', name: 'Constelação Familiar', description: 'Desc', longDescription: '', duration: '', price: null, currency: 'BRL',
    benefits: [], therapists: [],
  };

  beforeEach(async () => {
    servicesService = {
      list: vi.fn().mockName('ServicesService.list'),
      create: vi.fn().mockName('ServicesService.create'),
      update: vi.fn().mockName('ServicesService.update'),
      delete: vi.fn().mockName('ServicesService.delete')
    };
    servicesService.list.mockReturnValue(of([existing, withoutPrice]));
    servicesService.update.mockReturnValue(of(void 0));
    toast = { showSuccess: vi.fn(), showError: vi.fn(), showWarning: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [ServicesComponent],
      providers: [
        { provide: ServicesService, useValue: servicesService },
        { provide: ToastService, useValue: toast },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ServicesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  const lastUpdate = () => servicesService.update.mock.lastCall![1] as FormData;
  const sentBenefits = () => JSON.parse(lastUpdate().get('benefits') as string) as string[];

  it('shows the current benefits one per line when editing', () => {
    component.startEdit(existing);

    expect(component.form.value.benefits).toBe('Relaxamento\nSono');
  });

  it('keeps the service\'s benefits when only the description is edited', () => {
    component.startEdit(existing);
    component.form.patchValue({ description: 'Nova descrição' });

    component.submit();

    expect(sentBenefits()).toEqual(['Relaxamento', 'Sono']);
  });

  it('sends the benefits typed one per line, trimmed and without empty lines', () => {
    component.startEdit(existing);
    component.form.patchValue({ benefits: 'Relaxamento\n\n  Sono  \nEquilíbrio energético\n' });

    component.submit();

    expect(sentBenefits()).toEqual(['Relaxamento', 'Sono', 'Equilíbrio energético']);
  });

  it('sends no price when it is left empty ("sob consulta")', () => {
    component.startEdit(existing);
    component.form.patchValue({ price: null });

    component.submit();

    expect(lastUpdate().has('Price')).toBe(false);
  });

  it('says "atualizado" after saving an edit', () => {
    component.startEdit(existing);

    component.submit();

    expect(toast.showSuccess).toHaveBeenCalledWith('Serviço atualizado com sucesso!');
  });

  it('shows "Sob consulta" in the list for a service without a price', () => {
    expect(fixture.nativeElement.textContent).toContain('Sob consulta');
  });
});
