import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { errorInterceptor } from './error.interceptor';
import { ErrorService } from '../services/error.service';
import { LoggingService } from '../services/logging.service';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    const errorService = jasmine.createSpyObj('ErrorService', ['setError', 'getHttpErrorMessage', 'getUserFriendlyMessage']);
    errorService.getHttpErrorMessage.and.returnValue('Erro');
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: ErrorService, useValue: errorService },
        { provide: LoggingService, useValue: jasmine.createSpyObj('LoggingService', ['error', 'debug']) },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const fail500 = (url: string) =>
    httpMock.expectOne(url).flush('boom', { status: 500, statusText: 'Server Error' });

  it('retries a failed GET (safe to repeat)', fakeAsync(() => {
    http.get('/api/services').subscribe({ error: () => undefined });

    fail500('/api/services');
    tick(1000);
    fail500('/api/services');
    tick(2000);
    fail500('/api/services');
  }));

  it('never retries a failed POST, which could create duplicates', fakeAsync(() => {
    http.post('/api/contact', {}).subscribe({ error: () => undefined });

    fail500('/api/contact');
    tick(5000);
    httpMock.expectNone('/api/contact');
  }));

  it('never retries a failed PUT', fakeAsync(() => {
    http.put('/api/services/1', {}).subscribe({ error: () => undefined });

    fail500('/api/services/1');
    tick(5000);
    httpMock.expectNone('/api/services/1');
  }));
});
