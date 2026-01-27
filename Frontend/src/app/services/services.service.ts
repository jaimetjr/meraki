import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../environments/environment";
import type { Observable } from "rxjs";
import type { Service } from "../models/service.model";

@Injectable({ providedIn: "root" })
export class ServicesService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/services`;

  list(): Observable<Service[]> {
    return this.http.get<Service[]>(this.baseUrl);
  }

  get(id: string): Observable<Service> {
    return this.http.get<Service>(`${this.baseUrl}/${id}`);
  }

  getByCategory(categoryName: string): Observable<Service[]> {
    return this.http.get<Service[]>(`${this.baseUrl}/category/${categoryName}`);
  }

  create(payload: FormData): Observable<Service> {
    return this.http.post<Service>(this.baseUrl, payload);
  }

  update(id: string, payload: FormData): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

