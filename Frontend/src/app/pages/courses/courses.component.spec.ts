import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { CoursesComponent } from './courses.component';
import { CoursesService } from '../../services/courses.service';
import { MetaService } from '../../services/meta.service';
import type { Course } from '../../models/course.model';

describe('CoursesComponent (public page)', () => {
  let fixture: ComponentFixture<CoursesComponent>;

  // The API returns enum identifiers ("InscricoesAbertas"), not display text.
  const courses: Course[] = [
    { id: '1', title: 'Mindfulness', description: 'd', image: null, instructor: 'Ana', type: 'Curso', status: 'InscricoesAbertas', link: 'https://example.com/inscricao' },
    { id: '2', title: 'Trauma', description: 'd', image: null, instructor: 'Ana', type: 'Curso', status: 'EmBreve', link: 'https://example.com/trauma' },
  ];

  beforeEach(async () => {
    const coursesService = {
      list: vi.fn().mockName('CoursesService.list')
    };
    coursesService.list.mockReturnValue(of(courses));

    await TestBed.configureTestingModule({
      imports: [CoursesComponent],
      providers: [
        provideRouter([]),
        { provide: CoursesService, useValue: coursesService },
        { provide: MetaService, useValue: { updateMetaTags: vi.fn(), updateTitle: vi.fn(), setMeta: vi.fn() } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CoursesComponent);
    fixture.detectChanges();
  });

  const card = (title: string) =>
    Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('h3'))
      .find((h) => h.textContent?.includes(title))!
      .closest('div.border') as HTMLElement;

  it('shows the readable status text', () => {
    expect(card('Mindfulness').textContent).toContain('Inscrições Abertas');
    expect(card('Trauma').textContent).toContain('Em Breve');
  });

  it('offers the enrollment link only while enrollment is open', () => {
    expect(card('Mindfulness').querySelector('a[href="https://example.com/inscricao"]')).not.toBeNull();
    expect(card('Trauma').querySelector('a[href="https://example.com/trauma"]')).toBeNull();
  });

  it('renders courses without an image', () => {
    expect(card('Mindfulness').textContent).toContain('M');
    expect(card('Mindfulness').querySelector('img')).toBeNull();
  });
});
