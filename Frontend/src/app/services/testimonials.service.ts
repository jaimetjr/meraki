import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../environments/environment";
import type { Observable } from "rxjs";
import type { Testimonial } from "../models/testimonial.model";

@Injectable({ providedIn: "root" })
export class TestimonialsService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/testimonials`;

  list(): Observable<Testimonial[]> {
    return this.http.get<Testimonial[]>(this.baseUrl);
  }

  get(id: string): Observable<Testimonial> {
    return this.http.get<Testimonial>(`${this.baseUrl}/${id}`);
  }

  getTop(count: number): Observable<Testimonial[]> {
    return this.http.get<Testimonial[]>(`${this.baseUrl}/top/${count}`);
  }

  create(payload: FormData): Observable<Testimonial> {
    return this.http.post<Testimonial>(this.baseUrl, payload);
  }

  update(id: string, payload: FormData): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
