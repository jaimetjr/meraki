export interface Testimonial {
  id?: string;
  authorName: string;
  authorAvatarUrl?: string | null;
  authorBadge?: string;
  rating: number;
  content: string;
}
