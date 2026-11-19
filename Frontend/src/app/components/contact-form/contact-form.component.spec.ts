import { Component, input, output } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ContactFormComponent } from './contact-form.component';
import { ContactService } from '../../services/contact.service';
import { ServicesService } from '../../services/services.service';
import { ErrorService } from '../../services/error.service';
import { LoggingService } from '../../services/logging.service';
import { TurnstileWidgetComponent } from '../turnstile-widget/turnstile-widget.component';

/** Stands in for the Cloudflare widget, which would load a third-party script. */
@Component({ selector: 'app-turnstile-widget', standalone: true, template: '' })
class TurnstileWidgetStubComponent {
  siteKey = input<string>('');
  token = output<string | null>();
  resetCount = 0;
  reset() {
    this.resetCount++;
  }
}

describe('ContactFormComponent', () => {
  let component: ContactFormComponent;
  let fixture: ComponentFixture<ContactFormComponent>;
  let contactService: jasmine.SpyObj<ContactService>;
  let errorService: jasmine.SpyObj<ErrorService>;

  const validMessage = {
    name: 'Test User',
    email: 'test@example.com',
    phone: '1234567890',
    service: 'Terapia',
    message: 'Test message',
    consent: true,
  };

  beforeEach(async () => {
    contactService = jasmine.createSpyObj('ContactService', ['send']);
    errorService = jasmine.createSpyObj('ErrorService', ['setError', 'clearError']);
    const servicesService = jasmine.createSpyObj('ServicesService', ['list']);
    servicesService.list.and.returnValue(of([{ id: 's1', name: 'Terapia' }, { id: 's2', name: 'Mindfulness' }]));

    await TestBed.configureTestingModule({
      imports: [ContactFormComponent],
      providers: [
        { provide: ContactService, useValue: contactService },
        { provide: ServicesService, useValue: servicesService },
        { provide: ErrorService, useValue: errorService },
        { provide: LoggingService, useValue: jasmine.createSpyObj('LoggingService', ['error', 'info']) },
      ],
    })
      .overrideComponent(ContactFormComponent, {
        remove: { imports: [TurnstileWidgetComponent] },
        add: { imports: [TurnstileWidgetStubComponent] },
      })
      .compileComponents();

    fixture = TestBed.createComponent(ContactFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  const widget = () => fixture.debugElement.query((el) => el.componentInstance instanceof TurnstileWidgetStubComponent)
    .componentInstance as TurnstileWidgetStubComponent;

  it('requires name, email, phone, service and consent', () => {
    const form = component.contactForm;
    expect(form.get('name')?.hasError('required')).toBeTrue();
    expect(form.get('email')?.hasError('required')).toBeTrue();
    expect(form.get('phone')?.hasError('required')).toBeTrue();
    expect(form.get('service')?.hasError('required')).toBeTrue();
    expect(form.get('consent')?.hasError('required')).toBeTrue();
  });

  it('validates the email format', () => {
    const email = component.contactForm.get('email');
    email?.setValue('invalid-email');
    expect(email?.hasError('email')).toBeTrue();
    email?.setValue('valid@email.com');
    expect(email?.hasError('email')).toBeFalsy();
  });

  it('offers the services from the API plus "Outro"', () => {
    expect(component.serviceOptions()).toEqual(['Terapia', 'Mindfulness', 'Outro']);
  });

  it('does not submit before the captcha is solved', () => {
    component.contactForm.patchValue(validMessage);

    component.onSubmit();

    expect(contactService.send).not.toHaveBeenCalled();
    expect(component.captchaMissing()).toBeTrue();
  });

  it('sends the message with consent and the captcha token', () => {
    contactService.send.and.returnValue(of({ success: true, message: 'Enviado' }));
    component.contactForm.patchValue(validMessage);
    widget().token.emit('captcha-token');

    component.onSubmit();

    expect(contactService.send).toHaveBeenCalledWith({ ...validMessage, captchaToken: 'captcha-token' });
    expect(component.isSubmitted()).toBeTrue();
  });

  it('reports a failed submission, re-enables the form and asks for a new captcha', () => {
    contactService.send.and.returnValue(throwError(() => ({ userMessage: 'Falhou' })));
    component.contactForm.patchValue(validMessage);
    widget().token.emit('captcha-token');

    component.onSubmit();

    expect(errorService.setError).toHaveBeenCalledWith(jasmine.objectContaining({ message: 'Falhou', severity: 'error' }));
    expect(component.isSubmitting()).toBeFalse();
    expect(component.isSubmitted()).toBeFalse();
    expect(widget().resetCount).toBe(1);
  });

  it('does not submit an invalid form', () => {
    component.onSubmit();
    expect(contactService.send).not.toHaveBeenCalled();
  });

  it('resets the form', () => {
    component.contactForm.patchValue({ name: 'Test User' });
    component.isSubmitted.set(true);

    component.resetForm();

    expect(component.isSubmitted()).toBeFalse();
    expect(component.contactForm.value.name).toBeNull();
    expect(errorService.clearError).toHaveBeenCalled();
  });
});
