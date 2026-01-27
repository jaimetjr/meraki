import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';

/**
 * Logging service for application-wide logging.
 * In production, logs are suppressed to avoid exposing sensitive information.
 */
@Injectable({
  providedIn: 'root'
})
export class LoggingService {
  private readonly isProduction = environment.production;

  /**
   * Logs debug information (only in development)
   */
  debug(message: string, ...optionalParams: unknown[]): void {
    if (!this.isProduction) {
      console.debug(`[DEBUG] ${message}`, ...optionalParams);
    }
  }

  /**
   * Logs informational messages
   */
  info(message: string, ...optionalParams: unknown[]): void {
    if (!this.isProduction) {
      console.info(`[INFO] ${message}`, ...optionalParams);
    }
  }

  /**
   * Logs warning messages
   */
  warn(message: string, ...optionalParams: unknown[]): void {
    if (!this.isProduction) {
      console.warn(`[WARN] ${message}`, ...optionalParams);
    }
    // In production, you might want to send warnings to a logging service
  }

  /**
   * Logs error messages
   * In production, should be sent to error tracking service
   */
  error(message: string, error?: Error | unknown, ...optionalParams: unknown[]): void {
    if (!this.isProduction) {
      console.error(`[ERROR] ${message}`, error, ...optionalParams);
    }
    // TODO: In production, send to error tracking service (e.g., Sentry)
    // if (this.isProduction && error) {
    //   this.errorTrackingService.captureException(error);
    // }
  }

  /**
   * Logs with a specific log level
   */
  log(level: 'debug' | 'info' | 'warn' | 'error', message: string, ...optionalParams: unknown[]): void {
    switch (level) {
      case 'debug':
        this.debug(message, ...optionalParams);
        break;
      case 'info':
        this.info(message, ...optionalParams);
        break;
      case 'warn':
        this.warn(message, ...optionalParams);
        break;
      case 'error':
        this.error(message, ...optionalParams);
        break;
    }
  }
}

