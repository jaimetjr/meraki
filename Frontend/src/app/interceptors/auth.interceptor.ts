import { HttpInterceptorFn, HttpStatusCode } from "@angular/common/http";
import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { catchError, throwError } from "rxjs";
import { AuthService } from "../services/auth.service";

const AUTH_EXCLUDED_PATHS = ["/auth/login", "/auth/register"];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const shouldSkip = AUTH_EXCLUDED_PATHS.some((path) => req.url.includes(path));
  const token = authService.getToken();

  const authReq = !shouldSkip && token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((err) => {
      if (err.status === HttpStatusCode.Unauthorized || err.status === HttpStatusCode.Forbidden) {
        authService.logout(true);
      }
      return throwError(() => err);
    })
  );
};

