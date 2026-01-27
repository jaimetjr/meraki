import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../environments/environment";
import type { Observable } from "rxjs";
import type { User, CreateUserDto, UpdateUserDto } from "../models/user.model";

@Injectable({ providedIn: "root" })
export class UsersService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/users`;

  list(): Observable<User[]> {
    return this.http.get<User[]>(this.baseUrl);
  }

  get(id: string): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/${id}`);
  }

  create(payload: CreateUserDto): Observable<User> {
    return this.http.post<User>(this.baseUrl, payload);
  }

  update(id: string, payload: UpdateUserDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  updatePassword(id: string, newPassword: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/password`, { password: newPassword });
  }
}

