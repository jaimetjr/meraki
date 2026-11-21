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
  let therapistsService: jasmine.SpyObj<TherapistsService>;

  const existing: Therapist = {
    id: 't1', name: 'Ana', bio: 'Bio', experience: '5 anos', education: 'USP',
    specialties: [{ id: 's1', name: 'Ansiedade', description: '' }, { id: 's2', name: 'Trauma', description: '' }],
  } as Therapist;

  beforeEach(async () => {
    therapistsService = jasmine.createSpyObj('TherapistsService', ['list', 'create', 'update', 'delete']);
    therapistsService.list.and.returnValue(of([existing]));
    therapistsService.update.and.returnValue(of(void 0));
    const specialtiesService = jasmine.createSpyObj('SpecialtiesService', ['list']);
    // The specialties list failed to load, so the dropdown has no options.
    specialtiesService.list.and.returnValue(throwError(() => new Error('network')));
    const consultorioME = jasmine.createSpyObj('ConsultorioMEService', ['getProfessionals']);
    consultorioME.getProfessionals.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [TherapistsComponent],
      providers: [
        { provide: TherapistsService, useValue: therapistsService },
        { provide: SpecialtiesService, useValue: specialtiesService },
        { provide: ConsultorioMEService, useValue: consultorioME },
        { provide: ToastService, useValue: jasmine.createSpyObj('ToastService', ['showSuccess', 'showError', 'showWarning']) },
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

    const sent = therapistsService.update.calls.mostRecent().args[1] as FormData;
    const ids = (JSON.parse(sent.get('specialties') as string) as { id: string }[]).map((s) => s.id);
    expect(ids).toEqual(['s1', 's2']);
  });
});
