import { Injectable, signal } from '@angular/core';

/**
 * Service for managing global loading states.
 * Tracks the number of active operations (e.g., HTTP requests).
 */
@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private readonly _isLoading = signal<number>(0);

  /**
   * Signal indicating if any operations are currently loading
   */
  readonly isLoading = this._isLoading.asReadonly();

  /**
   * Increment loading counter
   */
  increment(): void {
    this._isLoading.update(count => count + 1);
  }

  /**
   * Decrement loading counter
   */
  decrement(): void {
    this._isLoading.update(count => Math.max(0, count - 1));
  }

  /**
   * Reset loading counter to zero
   */
  reset(): void {
    this._isLoading.set(0);
  }
}

