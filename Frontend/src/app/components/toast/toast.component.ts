import { Component, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ErrorService, type ErrorSeverity } from '../../services/error.service';

export interface Toast {
  id: string;
  message: string;
  severity: ErrorSeverity;
  duration?: number; // Auto-dismiss duration in ms. 0 = no auto-dismiss
}

/**
 * Toast notification component for displaying user feedback.
 * Displays errors, warnings, and info messages from the ErrorService.
 */
@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.css',
})
export class ToastComponent {
  private readonly errorService = inject(ErrorService);
  private readonly toasts = signal<Toast[]>([]);

  readonly visibleToasts = this.toasts.asReadonly();

  constructor() {
    // Watch for errors from ErrorService
    effect(() => {
      const error = this.errorService.currentError();
      if (error) {
        // Determine auto-dismiss duration based on severity
        let duration = 5000; // Default 5 seconds
        if (error.severity === 'error') {
          duration = 7000; // Errors auto-dismiss after 7 seconds
        } else if (error.severity === 'success') {
          duration = 4000; // Success messages dismiss after 4 seconds
        } else if (error.severity === 'warning') {
          duration = 6000; // Warnings dismiss after 6 seconds
        }
        
        this.show({
          message: error.message,
          severity: error.severity,
          duration,
        });
      }
    });
  }

  /**
   * Show a toast notification
   */
  show(toast: Omit<Toast, 'id'>): void {
    const id = `toast-${Date.now()}-${Math.random()}`;
    const duration = toast.duration ?? 5000;
    const newToast: Toast = {
      ...toast,
      id,
      duration,
    };

    this.toasts.update(toasts => [...toasts, newToast]);

    // Auto-dismiss if duration is set
    if (duration > 0) {
      setTimeout(() => {
        this.dismiss(id);
      }, duration);
    }
  }

  /**
   * Dismiss a toast by id
   */
  dismiss(id: string): void {
    this.toasts.update(toasts => toasts.filter(toast => toast.id !== id));
    // Clear error service if this was the current error
    const currentError = this.errorService.currentError();
    if (currentError) {
      this.errorService.clearError();
    }
  }

  /**
   * Get severity-based CSS classes
   */
  getSeverityClasses(severity: ErrorSeverity): string {
    const baseClasses = 'px-4 py-3 rounded-lg shadow-lg border flex items-start gap-3';
    const severityClasses = {
      error: 'bg-red-50 border-red-200 text-red-800',
      warning: 'bg-yellow-50 border-yellow-200 text-yellow-800',
      info: 'bg-blue-50 border-blue-200 text-blue-800',
      success: 'bg-green-50 border-green-200 text-green-800',
    };
    return `${baseClasses} ${severityClasses[severity]}`;
  }

  /**
   * Get icon for severity
   */
  getSeverityIcon(severity: ErrorSeverity): string {
    const icons = {
      error: 'M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z',
      warning: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z',
      info: 'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
      success: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',
    };
    return icons[severity];
  }
}

