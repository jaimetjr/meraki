import { Component, inject } from "@angular/core"
import { Router, RouterLink, NavigationEnd } from "@angular/router"
import { NgOptimizedImage } from "@angular/common"
import { ScrollService } from "../../services/scroll.service"
import { filter, take } from "rxjs/operators"

@Component({
  selector: "app-header",
  standalone: true,
  imports: [RouterLink, NgOptimizedImage],
  templateUrl: "./header.component.html",
  styleUrl: "./header.component.css",
})
export class HeaderComponent {
  isMenuOpen = false
  logoUrl = "https://hebbkx1anhila5yf.public.blob.vercel-storage.com/logo_white-xcKcT0DDe9L3p9gP0dexj9UyBLPudv.jpeg"
  private router = inject(Router);
  private scroll = inject(ScrollService);
  
  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen
  }

  go(e: Event, id: string, closeMenu = false): void {
    e.preventDefault();
    if (closeMenu) this.isMenuOpen = false;

    const onHome = this.router.url.split('?')[0] === '/';
    
    const scrollToSection = () => {
      // Use multiple animation frames to ensure DOM is ready
      requestAnimationFrame(() => {
        requestAnimationFrame(() => {
          this.scroll.scrollToId(id);
        });
      });
    };

    if (!onHome) {
      // Subscribe to navigation events BEFORE navigating
      const subscription = this.router.events
        .pipe(
          filter(event => event instanceof NavigationEnd),
          take(1)
        )
        .subscribe(() => {
          // Wait for lazy-loaded component to fully render
          setTimeout(scrollToSection, 200);
          subscription.unsubscribe();
        });

      // Navigate to home
      this.router.navigateByUrl('/');
    } else {
      // Already on home, scroll immediately
      scrollToSection();
    }
  }
}
