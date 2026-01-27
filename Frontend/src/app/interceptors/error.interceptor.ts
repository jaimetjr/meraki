import { HttpErrorResponse, HttpInterceptorFn, HttpStatusCode } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, retry, throwError, timer } from 'rxjs';
import { ErrorService } from '../services/error.service';
import { LoggingService } from '../services/logging.service';

const MAX_RETRIES = 2;
const RETRY_DELAY_MS = 1000;
const RETRIABLE_STATUS_CODES = [
  HttpStatusCode.RequestTimeout,
  HttpStatusCode.InternalServerError,
  HttpStatusCode.BadGateway,
  HttpStatusCode.ServiceUnavailable,
  HttpStatusCode.GatewayTimeout,
];

/**
 * HTTP interceptor for global error handling and retry logic.
 * Retries failed requests for transient errors and provides user-friendly error messages.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorService = inject(ErrorService);
  const loggingService = inject(LoggingService);

  return next(req).pipe(
    retry({
      count: MAX_RETRIES,
      delay: (error: HttpErrorResponse, retryCount: number) => {
        // Only retry for specific status codes
        if (!RETRIABLE_STATUS_CODES.includes(error.status)) {
          return throwError(() => error);
        }
        loggingService.debug(`Retrying request (${retryCount}/${MAX_RETRIES}): ${req.url}`);
        return timer(RETRY_DELAY_MS * retryCount);
      },
    }),
    catchError((error: HttpErrorResponse) => {
      const userMessage = errorService.getHttpErrorMessage(
        error.status,
        errorService.getUserFriendlyMessage(error)
      );

      // Log the error
      loggingService.error(
        `HTTP Error [${error.status}]: ${req.method} ${req.url}`,
        error,
        {
          status: error.status,
          statusText: error.statusText,
          url: req.url,
          method: req.method,
        }
      );

      // Set error in error service for global error handling
      errorService.setError({
        message: userMessage,
        severity: 'error',
        code: `HTTP_${error.status}`,
        details: {
          url: req.url,
          method: req.method,
          status: error.status,
        },
      });

      // Return user-friendly error
      return throwError(() => ({
        ...error,
        userMessage,
      }));
    })
  );
};

