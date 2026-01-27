import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../environments/environment";
import type { Observable } from "rxjs";
import type { Course } from "../models/course.model";

@Injectable({ providedIn: "root" })
export class CoursesService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/courses`;

  list(): Observable<Course[]> {
    return this.http.get<Course[]>(this.baseUrl);
  }

  get(id: string): Observable<Course> {
    return this.http.get<Course>(`${this.baseUrl}/${id}`);
  }

  getByStatus(status: string): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.baseUrl}/status/${status}`);
  }

  create(payload: FormData): Observable<Course> {
    return this.http.post<Course>(this.baseUrl, payload);
  }

  update(id: string, payload: FormData): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
