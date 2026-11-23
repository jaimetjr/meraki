import type { MockedObject } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { TestimonialsComponent } from './testimonials.component';
import { TestimonialsService } from '../../../services/testimonials.service';
import { ToastService } from '../../../services/toast.service';
import type { Testimonial } from '../../../models/testimonial.model';

describe('TestimonialsComponent (admin)', () => {
  let fixture: ComponentFixture<TestimonialsComponent>;
  let component: TestimonialsComponent;
  let testimonialsService: MockedObject<Pick<TestimonialsService, 'list' | 'create' | 'update' | 'delete'>>;

  const existing: Testimonial = {
    id: 't1', authorName: 'Juliana', authorAvatarUrl: 'https://cdn.test/juliana.png', rating: 5, content: 'Ótimo',
  } as Testimonial;

  beforeEach(async () => {
    testimonialsService = {
      list: vi.fn().mockName('TestimonialsService.list'),
      create: vi.fn().mockName('TestimonialsService.create'),
      update: vi.fn().mockName('TestimonialsService.update'),
      delete: vi.fn().mockName('TestimonialsService.delete')
    };
    testimonialsService.list.mockReturnValue(of([existing]));
    testimonialsService.create.mockReturnValue(of(existing));
    testimonialsService.update.mockReturnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [TestimonialsComponent],
      providers: [
        { provide: TestimonialsService, useValue: testimonialsService },
        { provide: ToastService, useValue: { showSuccess: vi.fn(), showError: vi.fn() } },
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

    const sent = testimonialsService.create.mock.lastCall![0] as FormData;
    expect(sent.get('authorName')).toBe('Ana');
    expect(sent.has('image')).toBe(false);
  });

  it('edits a testimonial without sending an image, so the current photo is kept', () => {
    component.startEdit(existing);
    component.form.patchValue({ content: 'Excelente' });

    component.submit();

    expect(testimonialsService.update).toHaveBeenCalled();
    const sent = testimonialsService.update.mock.lastCall![1] as FormData;
    expect(sent.get('content')).toBe('Excelente');
    expect(sent.has('image')).toBe(false);
  });
});
