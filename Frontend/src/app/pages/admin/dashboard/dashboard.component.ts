import { Component, OnDestroy, inject, signal, computed } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Subscription } from "rxjs";
import { AuthService } from "../../../services/auth.service";
import { AdminPanelComponent } from "../../../components/admin/admin-panel/admin-panel.component";
import type { AuthUser } from "../../../models/auth.model";
import type { AnalyticsData, TimeRange } from "../../../models/analytics.model";

@Component({
  selector: "app-admin-dashboard",
  standalone: true,
  imports: [CommonModule, AdminPanelComponent],
  templateUrl: "./dashboard.component.html",
  styleUrl: "./dashboard.component.css",
})
export class DashboardComponent implements OnDestroy {
  private authService = inject(AuthService);
  private sub: Subscription;

  user = signal<AuthUser | null>(this.authService.getCurrentUser());
  selectedTimeRange = signal<TimeRange>("last30days");

  // Mock data for different time ranges
  private mockData: Record<TimeRange, AnalyticsData> = {
    today: {
      pageViews: 1247,
      uniqueVisitors: 892,
      sessions: 1034,
      bounceRate: 42.5,
      avgSessionDuration: 185, // seconds
    },
    last7days: {
      pageViews: 8734,
      uniqueVisitors: 5234,
      sessions: 7123,
      bounceRate: 38.2,
      avgSessionDuration: 245, // seconds
    },
    last30days: {
      pageViews: 32456,
      uniqueVisitors: 18923,
      sessions: 26543,
      bounceRate: 35.8,
      avgSessionDuration: 312, // seconds
    },
  };

  analyticsData = computed(() => this.mockData[this.selectedTimeRange()]);

  pageViews = computed(() => this.formatNumber(this.analyticsData().pageViews));
  uniqueVisitors = computed(() => this.formatNumber(this.analyticsData().uniqueVisitors));
  sessions = computed(() => this.formatNumber(this.analyticsData().sessions));
  bounceRate = computed(() => `${this.analyticsData().bounceRate.toFixed(1)}%`);
  avgSessionDuration = computed(() => this.formatDuration(this.analyticsData().avgSessionDuration));

  constructor() {
    this.sub = this.authService.currentUser$.subscribe((u) => this.user.set(u));
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }

  setTimeRange(range: TimeRange) {
    this.selectedTimeRange.set(range);
  }

  private formatNumber(num: number): string {
    return new Intl.NumberFormat("pt-BR").format(num);
  }

  private formatDuration(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${minutes}:${secs.toString().padStart(2, "0")}`;
  }

  isTimeRangeActive(range: TimeRange): boolean {
    return this.selectedTimeRange() === range;
  }
}

