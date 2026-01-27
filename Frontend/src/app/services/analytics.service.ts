import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { LoggingService } from './logging.service';

declare global {
  interface Window {
    gtag?: (
      command: 'config' | 'event' | 'js' | 'set',
      targetId: string | Date,
      config?: Record<string, unknown>
    ) => void;
    dataLayer?: unknown[];
  }
}

/**
 * Google Analytics 4 service for tracking user interactions and page views.
 * Only runs in browser environment.
 */
@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly router = inject(Router);
  private readonly loggingService = inject(LoggingService);
  private readonly measurementId = 'G-XXXXXXXXXX'; // TODO: Replace with actual Google Analytics Measurement ID
  private initialized = false;

  /**
   * Initialize Google Analytics
   * Call this once in app initialization
   */
  initialize(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    if (this.initialized) {
      return;
    }

    try {
      // Load Google Analytics script
      const script1 = document.createElement('script');
      script1.async = true;
      script1.src = `https://www.googletagmanager.com/gtag/js?id=${this.measurementId}`;
      document.head.appendChild(script1);

      const script2 = document.createElement('script');
      script2.innerHTML = `
        window.dataLayer = window.dataLayer || [];
        function gtag(){dataLayer.push(arguments);}
        gtag('js', new Date());
        gtag('config', '${this.measurementId}');
      `;
      document.head.appendChild(script2);

      // Track page views on route changes
      this.router.events
        .pipe(filter(event => event instanceof NavigationEnd))
        .subscribe((event: NavigationEnd) => {
          this.trackPageView(event.urlAfterRedirects);
        });

      this.initialized = true;
      this.loggingService.debug('Google Analytics initialized');
    } catch (error) {
      this.loggingService.error('Failed to initialize Google Analytics', error);
    }
  }

  /**
   * Track a page view
   */
  trackPageView(url: string): void {
    if (!isPlatformBrowser(this.platformId) || !this.initialized) {
      return;
    }

    try {
      if (window.gtag) {
        window.gtag('config', this.measurementId, {
          page_path: url,
        });
      }
    } catch (error) {
      this.loggingService.error('Failed to track page view', error);
    }
  }

  /**
   * Track an event
   */
  trackEvent(
    eventName: string,
    eventParams?: {
      action?: string;
      category?: string;
      label?: string;
      value?: number;
      [key: string]: unknown;
    }
  ): void {
    if (!isPlatformBrowser(this.platformId) || !this.initialized) {
      return;
    }

    try {
      if (window.gtag) {
        window.gtag('event', eventName, eventParams);
      }
    } catch (error) {
      this.loggingService.error('Failed to track event', error);
    }
  }

  /**
   * Track form submission
   */
  trackFormSubmission(formName: string, success: boolean): void {
    this.trackEvent('form_submit', {
      form_name: formName,
      success: success,
    });
  }

  /**
   * Track button click
   */
  trackButtonClick(buttonName: string, location?: string): void {
    this.trackEvent('button_click', {
      button_name: buttonName,
      location: location || 'unknown',
    });
  }
}

