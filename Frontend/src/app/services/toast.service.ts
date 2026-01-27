import { Injectable, inject } from '@angular/core';
import { ErrorService, type ErrorSeverity } from './error.service';

/**
 * Service for showing toast notifications.
 * Provides convenient methods for displaying success, error, warning, and info messages.
 */
@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private readonly errorService = inject(ErrorService);

  /**
   * Show a success toast message
   */
  showSuccess(message: string, code?: string): void {
    this.errorService.setError({
      message,
      severity: 'success',
      code,
    });
  }

  /**
   * Show an error toast message
   */
  showError(message: string, code?: string): void {
    this.errorService.setError({
      message,
      severity: 'error',
      code,
    });
  }

  /**
   * Show a warning toast message
   */
  showWarning(message: string, code?: string): void {
    this.errorService.setError({
      message,
      severity: 'warning',
      code,
    });
  }

  /**
   * Show an info toast message
   */
  showInfo(message: string, code?: string): void {
    this.errorService.setError({
      message,
      severity: 'info',
      code,
    });
  }

  /**
   * Show a toast with custom severity
   */
  show(message: string, severity: ErrorSeverity, code?: string): void {
    this.errorService.setError({
      message,
      severity,
      code,
    });
  }

  /**
   * Clear the current toast/error
   */
  clear(): void {
    this.errorService.clearError();
  }
}

