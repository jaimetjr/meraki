import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ContactFormComponent } from './contact-form.component';
import { ContactService } from '../../services/contact.service';
import { ErrorService } from '../../services/error.service';
import { LoggingService } from '../../services/logging.service';
import { of, throwError } from 'rxjs';

describe('ContactFormComponent', () => {
  let component: ContactFormComponent;
  let fixture: ComponentFixture<ContactFormComponent>;
  let contactService: jasmine.SpyObj<ContactService>;
  let errorService: jasmine.SpyObj<ErrorService>;
  let loggingService: jasmine.SpyObj<LoggingService>;

  beforeEach(async () => {
    const contactServiceSpy = jasmine.createSpyObj('ContactService', ['send']);
    const errorServiceSpy = jasmine.createSpyObj('ErrorService', ['setError', 'clearError']);
    const loggingServiceSpy = jasmine.createSpyObj('LoggingService', ['error', 'info']);

    await TestBed.configureTestingModule({
      imports: [ContactFormComponent, ReactiveFormsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ContactService, useValue: contactServiceSpy },
        { provide: ErrorService, useValue: errorServiceSpy },
        { provide: LoggingService, useValue: loggingServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ContactFormComponent);
    component = fixture.componentInstance;
    contactService = TestBed.inject(ContactService) as jasmine.SpyObj<ContactService>;
    errorService = TestBed.inject(ErrorService) as jasmine.SpyObj<ErrorService>;
    loggingService = TestBed.inject(LoggingService) as jasmine.SpyObj<LoggingService>;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should mark form as invalid when empty', () => {
    expect(component.contactForm.invalid).toBeTrue();
  });

  it('should validate required fields', () => {
    const form = component.contactForm;
    expect(form.get('name')?.hasError('required')).toBeTrue();
    expect(form.get('email')?.hasError('required')).toBeTrue();
    expect(form.get('phone')?.hasError('required')).toBeTrue();
    expect(form.get('service')?.hasError('required')).toBeTrue();
  });

  it('should validate email format', () => {
    const emailControl = component.contactForm.get('email');
    emailControl?.setValue('invalid-email');
    expect(emailControl?.hasError('email')).toBeTrue();

    emailControl?.setValue('valid@email.com');
    expect(emailControl?.hasError('email')).toBeFalsy();
  });

  it('should submit form successfully', (done) => {
    const mockResponse = { success: true, message: 'Success' };
    contactService.send.and.returnValue(of(mockResponse));

    component.contactForm.patchValue({
      name: 'Test User',
      email: 'test@example.com',
      phone: '1234567890',
      service: 'acupuntura',
      message: 'Test message',
    });

    component.onSubmit();

    setTimeout(() => {
      expect(contactService.send).toHaveBeenCalled();
      expect(component.isSubmitted()).toBeTrue();
      expect(errorService.setError).toHaveBeenCalled();
      done();
    }, 100);
  });

  it('should handle form submission error', (done) => {
    const mockError = { userMessage: 'Error message' };
    contactService.send.and.returnValue(throwError(() => mockError));

    component.contactForm.patchValue({
      name: 'Test User',
      email: 'test@example.com',
      phone: '1234567890',
      service: 'acupuntura',
    });

    component.onSubmit();

    setTimeout(() => {
      expect(contactService.send).toHaveBeenCalled();
      expect(errorService.setError).toHaveBeenCalled();
      expect(component.isSubmitting()).toBeFalse();
      done();
    }, 100);
  });

  it('should not submit invalid form', () => {
    component.onSubmit();
    expect(contactService.send).not.toHaveBeenCalled();
  });

  it('should reset form', () => {
    component.contactForm.patchValue({
      name: 'Test User',
      email: 'test@example.com',
    });
    component.isSubmitted.set(true);

    component.resetForm();

    expect(component.isSubmitted()).toBeFalse();
    expect(component.contactForm.value.name).toBeNull();
    expect(errorService.clearError).toHaveBeenCalled();
  });
});

