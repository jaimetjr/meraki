import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { LoadingService } from '../services/loading.service';

/**
 * HTTP interceptor for managing loading states.
 * Tracks the number of active HTTP requests.
 */
export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);
  
  loadingService.increment();

  return next(req).pipe(
    finalize(() => {
      loadingService.decrement();
    })
  );
};

