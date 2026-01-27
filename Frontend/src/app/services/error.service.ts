import { Injectable, signal } from '@angular/core';

export type ErrorSeverity = 'error' | 'warning' | 'info' | 'success';

export interface AppError {
  message: string;
  severity: ErrorSeverity;
  timestamp: Date;
  code?: string;
  details?: unknown;
}

/**
 * Centralized error management service.
 * Provides error state signals and methods for error handling.
 */
@Injectable({
  providedIn: 'root'
})
export class ErrorService {
  private readonly _currentError = signal<AppError | null>(null);
  private readonly _errorHistory = signal<AppError[]>([]);

  /**
   * Signal representing the current error state
   */
  readonly currentError = this._currentError.asReadonly();

  /**
   * Signal representing error history
   */
  readonly errorHistory = this._errorHistory.asReadonly();

  /**
   * Sets an error
   */
  setError(error: Omit<AppError, 'timestamp'>): void {
    const appError: AppError = {
      ...error,
      timestamp: new Date()
    };
    this._currentError.set(appError);
    this._errorHistory.update(history => [...history, appError].slice(-50)); // Keep last 50 errors
  }

  /**
   * Clears the current error
   */
  clearError(): void {
    this._currentError.set(null);
  }

  /**
   * Gets user-friendly error message
   */
  getUserFriendlyMessage(error: unknown): string {
    if (error instanceof Error) {
      return error.message;
    }
    if (typeof error === 'string') {
      return error;
    }
    if (error && typeof error === 'object' && 'message' in error) {
      return String(error.message);
    }
    return 'Ocorreu um erro inesperado. Por favor, tente novamente.';
  }

  /**
   * Gets HTTP error message based on status code
   */
  getHttpErrorMessage(status: number, defaultMessage?: string): string {
    const messages: Record<number, string> = {
      400: 'Requisição inválida. Por favor, verifique os dados enviados.',
      401: 'Não autorizado. Por favor, faça login novamente.',
      403: 'Acesso negado. Você não tem permissão para realizar esta ação.',
      404: 'Recurso não encontrado.',
      408: 'Tempo de requisição esgotado. Por favor, tente novamente.',
      409: 'Conflito. O recurso já existe ou foi modificado.',
      422: 'Dados inválidos. Por favor, verifique os campos do formulário.',
      429: 'Muitas requisições. Por favor, aguarde um momento e tente novamente.',
      500: 'Erro interno do servidor. Por favor, tente novamente mais tarde.',
      502: 'Servidor temporariamente indisponível. Por favor, tente novamente.',
      503: 'Serviço temporariamente indisponível. Por favor, tente novamente.',
      504: 'Tempo de resposta do servidor esgotado. Por favor, tente novamente.',
    };

    return messages[status] || defaultMessage || 'Erro ao processar sua requisição. Por favor, tente novamente.';
  }
}

