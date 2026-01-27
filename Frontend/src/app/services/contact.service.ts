import { Injectable, inject } from "@angular/core"
import { HttpClient } from "@angular/common/http"
import { environment } from "../../environments/environment"
import type { Observable } from "rxjs"
import type { ContactRequest, ContactResponse } from "../models/contact.model"

@Injectable({ providedIn: "root" })
export class ContactService {
  private http = inject(HttpClient)
  private baseUrl = `${environment.apiBaseUrl}/contact`

  send(payload: ContactRequest): Observable<ContactResponse> {
    return this.http.post<ContactResponse>(this.baseUrl, payload)
  }
}


