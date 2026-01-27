import { Injectable, inject } from "@angular/core";
import { Router } from "@angular/router";
import { HttpClient } from "@angular/common/http";
import { BehaviorSubject, map, tap } from "rxjs";
import { environment } from "../../environments/environment";
import type { AuthUser, LoginRequest, LoginResponse } from "../models/auth.model";

const STORAGE_KEY = "meraki_auth";

@Injectable({ providedIn: "root" })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private baseUrl = `${environment.apiBaseUrl}/auth`;

  private currentUserSubject = new BehaviorSubject<AuthUser | null>(this.getStoredUser());
  currentUser$ = this.currentUserSubject.asObservable();

  login(payload: LoginRequest) {
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, payload).pipe(
      tap((res) => this.persistAuth(res)),
      map((res) => this.toAuthUser(res))
    );
  }

  logout(redirect: boolean = true) {
    localStorage.removeItem(STORAGE_KEY);
    this.currentUserSubject.next(null);
    if (redirect) {
      this.router.navigateByUrl("/admin/login");
    }
  }

  getToken(): string | null {
    const stored = this.getStoredUser();
    if (!stored) return null;
    if (this.isExpired(stored.expiresAt)) {
      // Clean up expired token but don't redirect here
      // Redirect will happen via isAuthenticated() check in guard or 401 in interceptor
      localStorage.removeItem(STORAGE_KEY);
      this.currentUserSubject.next(null);
      return null;
    }
    return stored.token;
  }

  isAuthenticated(): boolean {
    const stored = this.getStoredUser();
    if (!stored) return false;
    if (this.isExpired(stored.expiresAt)) {
      this.logout(true);
      return false;
    }
    return true;
  }

  getCurrentUser(): AuthUser | null {
    const stored = this.getStoredUser();
    if (!stored || this.isExpired(stored.expiresAt)) {
      return null;
    }
    return stored;
  }

  isSuperAdmin(): boolean {
    const user = this.getCurrentUser();
    return user?.role === "SuperAdmin";
  }

  private persistAuth(response: LoginResponse) {
    const authUser = this.toAuthUser(response);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(authUser));
    this.currentUserSubject.next(authUser);
  }

  private getStoredUser(): AuthUser | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as AuthUser;
    } catch {
      return null;
    }
  }

  private isExpired(expiresAt: string): boolean {
    const expiration = new Date(expiresAt).getTime();
    const now = Date.now();
    return Number.isNaN(expiration) || expiration <= now;
  }

  private toAuthUser(res: LoginResponse): AuthUser {
    return {
      token: res.token,
      email: res.email,
      role: res.role,
      userId: res.userId,
      expiresAt: res.expiresAt,
    };
  }
}

