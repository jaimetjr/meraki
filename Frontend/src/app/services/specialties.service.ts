import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../environments/environment";
import type { Observable } from "rxjs";
import type { Specialty } from "../models/specialty.model";

@Injectable({ providedIn: "root" })
export class SpecialtiesService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/specialties`;

  list(): Observable<Specialty[]> {
    return this.http.get<Specialty[]>(this.baseUrl);
  }

  get(id: string): Observable<Specialty> {
    return this.http.get<Specialty>(`${this.baseUrl}/${id}`);
  }

  getByName(name: string): Observable<Specialty[]> {
    return this.http.get<Specialty[]>(`${this.baseUrl}/name/${name}`);
  }

  create(payload: Specialty): Observable<Specialty> {
    return this.http.post<Specialty>(this.baseUrl, payload);
  }

  update(id: string, payload: Specialty): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

