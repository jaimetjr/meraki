import type { MockedObject } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TherapistsComponent } from './therapists.component';
import { TherapistsService } from '../../../services/therapists.service';
import { SpecialtiesService } from '../../../services/specialties.service';
import { ConsultorioMEService } from '../../../services/consultoriome.service';
import { ToastService } from '../../../services/toast.service';
import type { Therapist } from '../../../models/therapist.model';

describe('TherapistsComponent (admin)', () => {
  let fixture: ComponentFixture<TherapistsComponent>;
  let component: TherapistsComponent;
  let therapistsService: MockedObject<Pick<TherapistsService, 'list' | 'create' | 'update' | 'delete'>>;
  let consultorioME: MockedObject<Pick<ConsultorioMEService, 'getProfessionals'>>;

  const existing: Therapist = {
    id: 't1', name: 'Ana', bio: 'Bio', experience: '5 anos', education: 'USP', proId: 'pro-1',
    specialties: [{ id: 's1', name: 'Ansiedade', description: '' }, { id: 's2', name: 'Trauma', description: '' }],
  } as Therapist;

  beforeEach(async () => {
    therapistsService = {
      list: vi.fn().mockName('TherapistsService.list'),
      create: vi.fn().mockName('TherapistsService.create'),
      update: vi.fn().mockName('TherapistsService.update'),
      delete: vi.fn().mockName('TherapistsService.delete')
    };
    therapistsService.list.mockReturnValue(of([existing]));
    therapistsService.update.mockReturnValue(of(void 0));
    const specialtiesService = {
      list: vi.fn().mockName('SpecialtiesService.list')
    };
    // The specialties list failed to load, so the dropdown has no options.
    specialtiesService.list.mockReturnValue(throwError(() => new Error('network')));
    consultorioME = {
      getProfessionals: vi.fn().mockName('ConsultorioMEService.getProfessionals')
    };
    consultorioME.getProfessionals.mockReturnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [TherapistsComponent],
      providers: [
        { provide: TherapistsService, useValue: therapistsService },
        { provide: SpecialtiesService, useValue: specialtiesService },
        { provide: ConsultorioMEService, useValue: consultorioME },
        { provide: ToastService, useValue: { showSuccess: vi.fn(), showError: vi.fn(), showWarning: vi.fn() } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TherapistsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('keeps the therapist\'s specialties when only the bio is edited, even if the specialties list did not load', () => {
    component.startEdit(existing);
    component.form.patchValue({ bio: 'Nova bio' });

    component.submit();

    const sent = therapistsService.update.mock.lastCall![1] as FormData;
    const ids = (JSON.parse(sent.get('specialties') as string) as { id: string }[]).map((s) => s.id);
    expect(ids).toEqual(['s1', 's2']);
  });

  // The ConsultorioME integration is planned but switched off (environment.consultorioME).
  it('does not call ConsultorioME or show its field while the integration is off', () => {
    component.startEdit(existing);
    fixture.detectChanges();

    expect(consultorioME.getProfessionals).not.toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('input[formcontrolname=name]')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('select[formcontrolname=proId]')).toBeNull();
  });

  it('keeps the therapist\'s saved ConsultorioME id when editing while the integration is off', () => {
    component.startEdit(existing);
    component.form.patchValue({ bio: 'Nova bio' });

    component.submit();

    const sent = therapistsService.update.mock.lastCall![1] as FormData;
    expect(sent.get('proId')).toBe('pro-1');
  });
});
