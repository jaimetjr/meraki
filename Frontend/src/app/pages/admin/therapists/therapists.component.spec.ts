import type { MockedObject } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { type Observable, of, throwError } from 'rxjs';
import { TherapistsComponent } from './therapists.component';
import { TherapistsService } from '../../../services/therapists.service';
import { ServicesService } from '../../../services/services.service';
import { ConsultorioMEService } from '../../../services/consultoriome.service';
import { ToastService } from '../../../services/toast.service';
import type { Therapist } from '../../../models/therapist.model';
import type { Service } from '../../../models/service.model';

describe('TherapistsComponent (admin)', () => {
  let fixture: ComponentFixture<TherapistsComponent>;
  let component: TherapistsComponent;
  let therapistsService: MockedObject<Pick<TherapistsService, 'list' | 'create' | 'update' | 'delete'>>;
  let consultorioME: MockedObject<Pick<ConsultorioMEService, 'getProfessionals'>>;
  let toast: MockedObject<Pick<ToastService, 'showSuccess' | 'showError' | 'showWarning'>>;

  const service = (id: string, name: string) => ({ id, name, description: '', benefits: [], therapists: [], price: null, currency: 'BRL' }) as Service;
  const clinicServices = [service('s1', 'Reiki'), service('s2', 'Massagem'), service('s3', 'Hipnose')];

  const existing: Therapist = {
    id: 't1', name: 'Ana', bio: 'Bio', experience: '5 anos', education: 'USP', proId: 'pro-1',
    services: [{ id: 's1', name: 'Reiki' }, { id: 's2', name: 'Massagem' }],
  };

  // The therapist form loads the clinic's services on start; each test decides how that load goes.
  async function create(servicesList: Observable<Service[]>) {
    therapistsService = {
      list: vi.fn().mockName('TherapistsService.list'),
      create: vi.fn().mockName('TherapistsService.create'),
      update: vi.fn().mockName('TherapistsService.update'),
      delete: vi.fn().mockName('TherapistsService.delete')
    };
    therapistsService.list.mockReturnValue(of([existing]));
    therapistsService.update.mockReturnValue(of(void 0));
    consultorioME = {
      getProfessionals: vi.fn().mockName('ConsultorioMEService.getProfessionals')
    };
    consultorioME.getProfessionals.mockReturnValue(of([]));
    toast = { showSuccess: vi.fn(), showError: vi.fn(), showWarning: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [TherapistsComponent],
      providers: [
        { provide: TherapistsService, useValue: therapistsService },
        { provide: ServicesService, useValue: { list: vi.fn(() => servicesList) } },
        { provide: ConsultorioMEService, useValue: consultorioME },
        { provide: ToastService, useValue: toast },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TherapistsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  const lastUpdate = () => therapistsService.update.mock.lastCall![1] as FormData;
  const sentServiceIds = () => (JSON.parse(lastUpdate().get('services') as string) as { id: string }[]).map((s) => s.id);
  const checkbox = (serviceId: string) =>
    fixture.nativeElement.querySelector(`input[type=checkbox][value="${serviceId}"]`) as HTMLInputElement;

  it('keeps the therapist\'s services when only the bio is edited, even if the services list did not load', async () => {
    await create(throwError(() => new Error('network')));
    component.startEdit(existing);
    component.form.patchValue({ bio: 'Nova bio' });

    component.submit();

    expect(sentServiceIds()).toEqual(['s1', 's2']);
  });

  it('shows the clinic\'s services as checkboxes, ticked for the ones the therapist offers', async () => {
    await create(of(clinicServices));
    component.startEdit(existing);
    fixture.detectChanges();

    expect(checkbox('s1').checked).toBe(true);
    expect(checkbox('s2').checked).toBe(true);
    expect(checkbox('s3').checked).toBe(false);
  });

  it('sends the services as they are ticked', async () => {
    await create(of(clinicServices));
    component.startEdit(existing);
    fixture.detectChanges();

    checkbox('s1').click();
    checkbox('s3').click();
    component.submit();

    expect(sentServiceIds()).toEqual(['s2', 's3']);
  });

  it('says "atualizado" after saving an edit', async () => {
    await create(of(clinicServices));
    component.startEdit(existing);

    component.submit();

    expect(toast.showSuccess).toHaveBeenCalledWith('Terapeuta atualizado com sucesso!');
  });

  // The ConsultorioME integration is planned but switched off (environment.consultorioME).
  it('does not call ConsultorioME or show its field while the integration is off', async () => {
    await create(of(clinicServices));
    component.startEdit(existing);
    fixture.detectChanges();

    expect(consultorioME.getProfessionals).not.toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('input[formcontrolname=name]')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('select[formcontrolname=proId]')).toBeNull();
  });

  it('keeps the therapist\'s saved ConsultorioME id when editing while the integration is off', async () => {
    await create(of(clinicServices));
    component.startEdit(existing);
    component.form.patchValue({ bio: 'Nova bio' });

    component.submit();

    expect(lastUpdate().get('proId')).toBe('pro-1');
  });
});
