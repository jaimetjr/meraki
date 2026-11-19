import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ContactService } from './contact.service';
import { environment } from '../../environments/environment';
import type { ContactMessage, ContactRequest, ContactResponse } from '../models/contact.model';

describe('ContactService', () => {
  let service: ContactService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/contact`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ContactService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ContactService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('posts the message with consent and captcha token', () => {
    const request: ContactRequest = {
      name: 'Test User',
      email: 'test@example.com',
      phone: '1234567890',
      service: 'Terapia',
      message: 'Test message',
      consent: true,
      captchaToken: 'token-1',
    };
    const response: ContactResponse = { success: true, message: 'ok' };
    let received: ContactResponse | undefined;

    service.send(request).subscribe((r) => (received = r));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(response);
    expect(received).toEqual(response);
  });

  it('lists the inbox', () => {
    const messages: ContactMessage[] = [{
      id: '1', name: 'Ana', email: 'ana@example.com', phone: '1', service: 'Terapia',
      message: 'Oi', isRead: false, createdAt: '2026-11-20T10:00:00Z',
    }];
    let received: ContactMessage[] | undefined;

    service.list().subscribe((m) => (received = m));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush(messages);
    expect(received).toEqual(messages);
  });

  it('marks a message read or unread', () => {
    service.markRead('42', true).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/42/read`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ isRead: true });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('deletes a message', () => {
    service.delete('42').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/42`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null, { status: 204, statusText: 'No Content' });
  });
});
