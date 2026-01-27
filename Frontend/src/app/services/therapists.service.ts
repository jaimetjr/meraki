import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../environments/environment";
import type { Observable } from "rxjs";
import type { Therapist } from "../models/therapist.model";

@Injectable({ providedIn: "root" })
export class TherapistsService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/therapists`;

  list(): Observable<Therapist[]> {
    return this.http.get<Therapist[]>(this.baseUrl);
  }

  get(id: string): Observable<Therapist> {
    return this.http.get<Therapist>(`${this.baseUrl}/${id}`);
  }

  getBySpecialty(name: string): Observable<Therapist[]> {
    return this.http.get<Therapist[]>(`${this.baseUrl}/specialty/${name}`);
  }

  create(payload: FormData): Observable<Therapist> {
    return this.http.post<Therapist>(this.baseUrl, payload);
  }

  update(id: string, payload: FormData): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
