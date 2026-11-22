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

describe('errorInterceptor messages', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: LoggingService, useValue: jasmine.createSpyObj('LoggingService', ['error', 'debug']) },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function messageFor(answer: (request: ReturnType<HttpTestingController['expectOne']>) => void): string {
    let message = '';
    http.post('/api/x', {}).subscribe({ error: (e) => (message = e.userMessage) });
    answer(httpMock.expectOne('/api/x'));
    return message;
  }

  it("shows the API's own reason when it rejects a request", () => {
    const message = messageFor((r) => r.flush({ title: 'Invalid request', detail: "'Nome' deve ser informado." }, { status: 400, statusText: 'Bad Request' }));
    expect(message).toBe("'Nome' deve ser informado.");
  });

  it("shows the API's own reason for a conflict", () => {
    const detail = 'Esta categoria é usada por 1 serviço(s). Mova-os para outra categoria antes de excluí-la.';
    expect(messageFor((r) => r.flush({ detail }, { status: 409, statusText: 'Conflict' }))).toBe(detail);
  });

  it('keeps the generic message for a 400 that carries no reason', () => {
    const message = messageFor((r) => r.flush({ title: 'One or more validation errors occurred.', errors: { isActive: ['x'] } }, { status: 400, statusText: 'Bad Request' }));
    expect(message).toBe('Requisição inválida. Por favor, verifique os dados enviados.');
  });

  it('keeps the generic message for a server error, whatever its body says', () => {
    const message = messageFor((r) => r.flush({ detail: 'An unexpected error occurred.' }, { status: 500, statusText: 'Server Error' }));
    expect(message).toBe('Erro interno do servidor. Por favor, tente novamente mais tarde.');
  });

  it("says the server could not be reached instead of the browser's English error", () => {
    const message = messageFor((r) => r.error(new ProgressEvent('error')));
    expect(message).toBe('Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.');
  });
});
