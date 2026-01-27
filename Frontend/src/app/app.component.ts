import { AfterViewInit, Component, OnDestroy, inject, OnInit, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { HeaderComponent } from "./components/header/header.component";
import { ToastComponent } from "./components/toast/toast.component";
import { ScrollService } from './services/scroll.service';
import { LoggingService } from './services/logging.service';
import { AnalyticsService } from './services/analytics.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, ToastComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit, AfterViewInit, OnDestroy {
  title = 'Meraki Saúde Integrativa';

  private readonly sections = ['about','therapists','services','contact'];
  private readonly router = inject(Router);
  private readonly scroll = inject(ScrollService);
  private readonly loggingService = inject(LoggingService);
  private readonly analyticsService = inject(AnalyticsService);
  private navSub?: Subscription;

  showHeader = signal(true);

  ngOnInit(): void {
    // Initialize analytics
    this.analyticsService.initialize();
    this.updateHeaderVisibility(this.router.url);
    this.navSub = this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.updateHeaderVisibility(event.urlAfterRedirects);
      }
    });
  }

  ngAfterViewInit(): void {
    this.initHeaderHeightVar();   // sets --header-h and keeps it updated
    this.restoreLastSection();    // brings user back after refresh
    this.observeVisibleSection(); // OPTIONAL: comment out if you only want click-memory
  }

  ngOnDestroy(): void {
    this.navSub?.unsubscribe();
  }

  /** Keeps CSS var --header-h equal to the real header height (desktop/mobile). */
  private initHeaderHeightVar() {
    const header = document.querySelector('app-header') as HTMLElement | null;
    if (!header) return;

    const setVar = () => {
      const h = Math.round(header.getBoundingClientRect().height);
      document.documentElement.style.setProperty('--header-h', `${h}px`);
    };

    setVar();
    new ResizeObserver(setVar).observe(header);
    addEventListener('orientationchange', setVar);
  }

  /** Restores last section after reload, but only on the home route. */
  private restoreLastSection() {
    if (this.router.url !== '/') return;
    const id = localStorage.getItem('lastSection');
    if (!id) return;
    this.loggingService.debug('Restoring last section:', id);
    requestAnimationFrame(() => this.scroll.scrollToId(id));
  }

  /** OPTIONAL: remembers the section the user is currently viewing while scrolling. */
  private observeVisibleSection() {
    if (this.router.url !== '/') return;

    const io = new IntersectionObserver((entries) => {
      const visible = entries
        .filter(e => e.isIntersecting)
        .sort((a,b) => b.intersectionRatio - a.intersectionRatio)[0];
      if (visible?.target?.id) localStorage.setItem('lastSection', visible.target.id);
    }, { threshold: [0.5] });

    this.sections.forEach(id => {
      const el = document.getElementById(id);
      if (el) io.observe(el);
    });
  }

  private updateHeaderVisibility(url: string) {
    const isAdmin = url.startsWith('/admin');
    this.showHeader.set(!isAdmin);
  }
}
