import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ContactService } from './contact.service';
import { environment } from '../../environments/environment';
import type { ContactRequest, ContactResponse } from '../models/contact.model';

describe('ContactService', () => {
  let service: ContactService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ContactService],
    });
    service = TestBed.inject(ContactService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should send contact request', () => {
    const mockRequest: ContactRequest = {
      name: 'Test User',
      email: 'test@example.com',
      phone: '1234567890',
      service: 'acupuntura',
      message: 'Test message',
    };

    const mockResponse: ContactResponse = {
      success: true,
      message: 'Message sent successfully',
    };

    service.send(mockRequest).subscribe((response) => {
      expect(response).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/contact`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockRequest);
    req.flush(mockResponse);
  });

  it('should handle error response', () => {
    const mockRequest: ContactRequest = {
      name: 'Test User',
      email: 'test@example.com',
      phone: '1234567890',
      service: 'acupuntura',
    };

    service.send(mockRequest).subscribe({
      next: () => fail('should have failed'),
      error: (error) => {
        expect(error.status).toBe(500);
      },
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/contact`);
    req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });
  });
});

