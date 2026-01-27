import { Component, OnDestroy, inject, signal, computed, effect } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Router, RouterLink, RouterOutlet, NavigationEnd } from "@angular/router";
import { Subscription, filter } from "rxjs";
import { AuthService } from "../../../services/auth.service";
import type { AuthUser } from "../../../models/auth.model";

const NAV_ITEMS = [
  { path: "/admin/dashboard", label: "Acessos" },
  { path: "/admin/services", label: "Serviços" },
  { path: "/admin/courses", label: "Cursos" },
  { path: "/admin/therapists", label: "Terapeutas" },
  { path: "/admin/categories", label: "Categorias" },
  { path: "/admin/benefits", label: "Benefícios" },
  { path: "/admin/specialties", label: "Especialidades" },
  { path: "/admin/testimonials", label: "Depoimentos" },
];

const SUPER_ADMIN_NAV_ITEMS = [
  ...NAV_ITEMS,
  { path: "/admin/users", label: "Usuários" },
];

@Component({
  selector: "app-admin-shell",
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  templateUrl: "./admin-shell.component.html",
  styleUrl: "./admin-shell.component.css",
})
export class AdminShellComponent implements OnDestroy {
  private authService = inject(AuthService);
  private router = inject(Router);
  private sub: Subscription;
  private routerSub: Subscription;

  user = signal<AuthUser | null>(this.authService.getCurrentUser());
  menuOpen = signal(false);
  currentUrl = signal<string>(this.router.url);
  
  navItems = computed(() => {
    return this.authService.isSuperAdmin() ? SUPER_ADMIN_NAV_ITEMS : NAV_ITEMS;
  });

  constructor() {
    this.sub = this.authService.currentUser$.subscribe((u) => this.user.set(u));
    
    // Track router URL changes
    this.routerSub = this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.currentUrl.set(event.url);
      });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.routerSub?.unsubscribe();
  }

  logout() {
    this.authService.logout();
  }

  toggleMenu() {
    this.menuOpen.update((v) => !v);
  }

  isActive(path: string) {
    return this.currentUrl().startsWith(path);
  }
}

