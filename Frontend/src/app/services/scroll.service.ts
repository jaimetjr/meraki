import { Injectable, Inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';

@Injectable({ providedIn: 'root' })
export class ScrollService {
  constructor(@Inject(DOCUMENT) private doc: Document) {}

  scrollToId(id: string, retries = 0): void {
    const section = this.doc.getElementById(id);
    if (!section) {
      // Retry if section not found (might be lazy loading)
      if (retries < 5) {
        setTimeout(() => this.scrollToId(id, retries + 1), 100);
      }
      return;
    }
    
    // Find the element with data-scroll-anchor attribute within the section
    const target = (section.querySelector('[data-scroll-anchor]') as HTMLElement) ?? section;
    
    if (!target) {
      return;
    }

    // Get header height from CSS variable or default to 64px
    const headerHeightStr = getComputedStyle(this.doc.documentElement)
      .getPropertyValue('--header-h')
      .trim()
      .replace('px', '');
    const headerHeight = parseInt(headerHeightStr) || 64;

    // Calculate the target position accounting for the fixed header
    const targetPosition = target.getBoundingClientRect().top + window.pageYOffset - headerHeight;

    // Smooth scroll to the calculated position
    window.scrollTo({
      top: Math.max(0, targetPosition),
      behavior: 'smooth'
    });
  }
}
