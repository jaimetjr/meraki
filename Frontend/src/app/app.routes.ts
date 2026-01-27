import type { Routes } from "@angular/router"
import { MaintenanceComponent } from "./pages/maintenance/maintenance.component"
import { MaintenanceGuard } from "./guards/maintenance.guard"
import { AuthGuard } from "./guards/auth.guard"
import { SuperAdminGuard } from "./guards/super-admin.guard"
import { AdminShellComponent } from "./pages/admin/admin-shell/admin-shell.component"

export const routes: Routes = [
  { 
    path: "", 
    loadComponent: () => import("./pages/home/home.component").then(m => m.HomeComponent),
    canActivate: [MaintenanceGuard] 
  },
  { 
    path: 'cursos', 
    loadComponent: () => import("./pages/courses/courses.component").then(m => m.CoursesComponent),
    canActivate: [MaintenanceGuard] 
  },
  {
    path: 'admin/login',
    loadComponent: () => import("./pages/admin/login/login.component").then(m => m.LoginComponent),
    canActivate: [MaintenanceGuard]
  },
  {
    path: 'admin',
    component: AdminShellComponent,
    canActivate: [MaintenanceGuard, AuthGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', loadComponent: () => import("./pages/admin/dashboard/dashboard.component").then(m => m.DashboardComponent) },
      { path: 'services', loadComponent: () => import("./pages/admin/services/services.component").then(m => m.ServicesComponent) },
      { path: 'courses', loadComponent: () => import("./pages/admin/courses/courses.component").then(m => m.CoursesComponent) },
      { path: 'therapists', loadComponent: () => import("./pages/admin/therapists/therapists.component").then(m => m.TherapistsComponent) },
      { path: 'categories', loadComponent: () => import("./pages/admin/categories/categories.component").then(m => m.CategoriesComponent) },
      { path: 'benefits', loadComponent: () => import("./pages/admin/benefits/benefits.component").then(m => m.BenefitsComponent) },
      { path: 'specialties', loadComponent: () => import("./pages/admin/specialties/specialties.component").then(m => m.SpecialtiesComponent) },
      { path: 'testimonials', loadComponent: () => import("./pages/admin/testimonials/testimonials.component").then(m => m.TestimonialsComponent) },
      { path: 'users', loadComponent: () => import("./pages/admin/users/users.component").then(m => m.UsersComponent), canActivate: [SuperAdminGuard] },
    ]
  },
  { path: 'maintenance', component: MaintenanceComponent },
  { path: "**", redirectTo: "", canActivate: [MaintenanceGuard] },
]
