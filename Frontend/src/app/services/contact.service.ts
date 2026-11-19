import { Injectable, inject } from "@angular/core"
import { HttpClient } from "@angular/common/http"
import { environment } from "../../environments/environment"
import type { Observable } from "rxjs"
import type { ContactMessage, ContactRequest, ContactResponse } from "../models/contact.model"

@Injectable({ providedIn: "root" })
export class ContactService {
  private http = inject(HttpClient)
  private baseUrl = `${environment.apiBaseUrl}/contact`

  /** Public: submit the contact form. */
  send(payload: ContactRequest): Observable<ContactResponse> {
    return this.http.post<ContactResponse>(this.baseUrl, payload)
  }

  /** Admin: the inbox, newest first. */
  list(): Observable<ContactMessage[]> {
    return this.http.get<ContactMessage[]>(this.baseUrl)
  }

  markRead(id: string, isRead: boolean): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/read`, { isRead })
  }

  /** Admin: permanently deletes the message. */
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`)
  }
}
