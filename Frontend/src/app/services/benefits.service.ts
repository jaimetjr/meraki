import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../environments/environment";
import type { Observable } from "rxjs";
import type { Benefit } from "../models/benefit.model";

@Injectable({ providedIn: "root" })
export class BenefitsService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/benefits`;

  list(): Observable<Benefit[]> {
    return this.http.get<Benefit[]>(this.baseUrl);
  }

  get(id: string): Observable<Benefit> {
    return this.http.get<Benefit>(`${this.baseUrl}/${id}`);
  }

  search(keyword: string): Observable<Benefit[]> {
    return this.http.get<Benefit[]>(`${this.baseUrl}/search/${keyword}`);
  }

  create(payload: Benefit): Observable<Benefit> {
    return this.http.post<Benefit>(this.baseUrl, payload);
  }

  update(id: string, payload: Benefit): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

