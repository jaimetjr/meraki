import { Injectable, inject } from "@angular/core"
import { HttpClient } from "@angular/common/http"
import { environment } from "../../environments/environment"
import type { Observable } from "rxjs"
import type { Service } from "../models/service.model"

@Injectable({ providedIn: "root" })
export class ServiceManager {
  private http = inject(HttpClient)
  private baseUrl = `${environment.apiBaseUrl}/services`

  list(): Observable<Service[]> {
    return this.http.get<Service[]>(this.baseUrl)
  }

  getById(id: string): Observable<Service> {
    return this.http.get<Service>(`${this.baseUrl}/${id}`)
  }

  listByCategory(category: string): Observable<Service[]> {
    return this.http.get<Service[]>(`${this.baseUrl}?category=${encodeURIComponent(category)}`)
  }
}


