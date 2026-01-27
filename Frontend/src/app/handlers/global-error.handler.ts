import { ErrorHandler, inject } from '@angular/core';
import { LoggingService } from '../services/logging.service';
import { ErrorService } from '../services/error.service';

/**
 * Global error handler for unhandled errors in the application.
 * Catches errors that escape component error handlers and provides centralized logging.
 */
export class GlobalErrorHandler implements ErrorHandler {
  private readonly loggingService = inject(LoggingService);
  private readonly errorService = inject(ErrorService);

  handleError(error: Error | unknown): void {
    // Extract error message
    const message = this.errorService.getUserFriendlyMessage(error);

    // Log the error
    this.loggingService.error('Unhandled error occurred', error);

    // Set error in error service for UI display
    this.errorService.setError({
      message: 'Ocorreu um erro inesperado. Por favor, recarregue a página.',
      severity: 'error',
      code: 'UNHANDLED_ERROR',
      details: error,
    });

    // In production, you would send this to an error tracking service
    // Example: this.errorTrackingService.captureException(error);
  }
}

