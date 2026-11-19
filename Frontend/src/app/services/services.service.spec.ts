import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ServicesService } from './services.service';
import { environment } from '../../environments/environment';

describe('ServicesService', () => {
  let service: ServicesService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(ServicesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('lists services', () => {
    service.list().subscribe();
    httpMock.expectOne(`${environment.apiBaseUrl}/services`).flush([]);
  });

  it('filters by category using the category route with the name encoded', () => {
    service.getByCategory('Terapia Integrativa').subscribe();
    httpMock.expectOne(`${environment.apiBaseUrl}/services/category/Terapia%20Integrativa`).flush([]);
  });
});
