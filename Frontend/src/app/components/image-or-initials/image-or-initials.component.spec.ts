import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ImageOrInitialsComponent, initialsOf } from './image-or-initials.component';

describe('initialsOf', () => {
  it('uses the first and last names, skipping titles like "Dra."', () => {
    expect(initialsOf('Dra. Ana Souza')).toBe('AS');
    expect(initialsOf('Carlos Lima')).toBe('CL');
    expect(initialsOf('respiração consciente guiada')).toBe('RG');
  });

  it('uses one letter for a single word and "?" for nothing', () => {
    expect(initialsOf('Mindfulness')).toBe('M');
    expect(initialsOf('   ')).toBe('?');
  });
});

describe('ImageOrInitialsComponent', () => {
  let fixture: ComponentFixture<ImageOrInitialsComponent>;
  const element = () => fixture.nativeElement as HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ImageOrInitialsComponent] }).compileComponents();
    fixture = TestBed.createComponent(ImageOrInitialsComponent);
    fixture.componentRef.setInput('label', 'Carlos Lima');
    fixture.componentRef.setInput('alt', 'Foto de Carlos Lima');
  });

  it('shows the initials when there is no image', () => {
    fixture.componentRef.setInput('src', null);
    fixture.detectChanges();

    expect(element().querySelector('img')).toBeNull();
    expect(element().textContent?.trim()).toBe('CL');
  });

  it('shows the image when there is one', () => {
    fixture.componentRef.setInput('src', 'https://cdn.test/carlos.png');
    fixture.detectChanges();

    expect(element().querySelector('img')?.getAttribute('src')).toBe('https://cdn.test/carlos.png');
  });

  it('falls back to the initials when the image fails to load', () => {
    fixture.componentRef.setInput('src', 'https://cdn.test/missing.png');
    fixture.detectChanges();

    element().querySelector('img')!.dispatchEvent(new Event('error'));
    fixture.detectChanges();

    expect(element().querySelector('img')).toBeNull();
    expect(element().textContent?.trim()).toBe('CL');
  });
});
