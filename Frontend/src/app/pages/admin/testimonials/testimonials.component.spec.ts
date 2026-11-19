import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { TestimonialsComponent } from './testimonials.component';
import { TestimonialsService } from '../../../services/testimonials.service';
import { ToastService } from '../../../services/toast.service';
import type { Testimonial } from '../../../models/testimonial.model';

describe('TestimonialsComponent (admin)', () => {
  let fixture: ComponentFixture<TestimonialsComponent>;
  let component: TestimonialsComponent;
  let testimonialsService: jasmine.SpyObj<TestimonialsService>;

  const existing: Testimonial = {
    id: 't1', authorName: 'Juliana', authorAvatarUrl: 'https://cdn.test/juliana.png', rating: 5, content: 'Ótimo',
  } as Testimonial;

  beforeEach(async () => {
    testimonialsService = jasmine.createSpyObj('TestimonialsService', ['list', 'create', 'update', 'delete']);
    testimonialsService.list.and.returnValue(of([existing]));
    testimonialsService.create.and.returnValue(of(existing));
    testimonialsService.update.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [TestimonialsComponent],
      providers: [
        { provide: TestimonialsService, useValue: testimonialsService },
        { provide: ToastService, useValue: jasmine.createSpyObj('ToastService', ['showSuccess', 'showError']) },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TestimonialsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('creates a testimonial without a photo (the photo is optional)', () => {
    component.startCreate();
    component.form.patchValue({ authorName: 'Ana', rating: 4, content: 'Muito bom' });

    component.submit();

    const sent = testimonialsService.create.calls.mostRecent().args[0] as FormData;
    expect(sent.get('authorName')).toBe('Ana');
    expect(sent.has('image')).toBeFalse();
  });

  it('edits a testimonial without sending an image, so the current photo is kept', () => {
    component.startEdit(existing);
    component.form.patchValue({ content: 'Excelente' });

    component.submit();

    expect(testimonialsService.update).toHaveBeenCalled();
    const sent = testimonialsService.update.calls.mostRecent().args[1] as FormData;
    expect(sent.get('content')).toBe('Excelente');
    expect(sent.has('image')).toBeFalse();
  });
});
