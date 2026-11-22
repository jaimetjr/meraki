import { type ApplicationConfig, LOCALE_ID, ErrorHandler, isDevMode, provideZoneChangeDetection } from "@angular/core"
import { registerLocaleData } from "@angular/common"
import localePt from "@angular/common/locales/pt"
import { provideRouter, withViewTransitions, withComponentInputBinding, withInMemoryScrolling } from "@angular/router"
import { provideHttpClient, withInterceptors } from "@angular/common/http"
import { provideServiceWorker } from "@angular/service-worker"
import { routes } from "./app.routes"
import { MaintenanceService } from "./services/maintenance.service"
import { errorInterceptor } from "./interceptors/error.interceptor"
import { loadingInterceptor } from "./interceptors/loading.interceptor"
import { authInterceptor } from "./interceptors/auth.interceptor"
import { GlobalErrorHandler } from "./handlers/global-error.handler"

registerLocaleData(localePt)

export const appConfig: ApplicationConfig = {
  providers: [
    // Angular 21+ defaults to zoneless change detection; this app still relies on zone.js.
    provideZoneChangeDetection(),
    provideRouter(
      routes,
      withViewTransitions(),
      withComponentInputBinding(),
      withInMemoryScrolling({
        anchorScrolling: 'enabled',
        scrollPositionRestoration: 'enabled',
      })
    ),
    provideHttpClient(
      withInterceptors([
        loadingInterceptor,
        errorInterceptor,
        authInterceptor,
      ])
    ),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000'
    }),
    { provide: LOCALE_ID, useValue: 'pt-BR' },
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    MaintenanceService,
  ],
}
