export type TimeRange = 'today' | 'last7days' | 'last30days';

export interface AnalyticsData {
  pageViews: number;
  uniqueVisitors: number;
  sessions: number;
  bounceRate: number; // Percentage (0-100)
  avgSessionDuration: number; // In seconds
}

export interface AnalyticsMetric {
  label: string;
  value: string | number;
  change?: number; // Percentage change from previous period (optional)
}

