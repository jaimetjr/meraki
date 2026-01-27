import { Injectable, inject } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';

export interface MetaTags {
  title?: string;
  description?: string;
  keywords?: string;
  image?: string;
  url?: string;
  type?: string;
}

/**
 * Service for managing dynamic meta tags for SEO.
 * Updates page titles, descriptions, and Open Graph tags per route.
 */
@Injectable({
  providedIn: 'root'
})
export class MetaService {
  private readonly meta = inject(Meta);
  private readonly title = inject(Title);
  private readonly defaultTitle = 'Meraki Saúde Integrativa - Cuidando do seu bem-estar de forma integral';
  private readonly defaultDescription = 'Meraki Saúde Integrativa oferece tratamentos terapêuticos personalizados com acupuntura, fisioterapia, massoterapia e psicoterapia. Agende sua consulta hoje!';
  private readonly defaultImage = 'https://merakisaude.com.br/assets/og-image.jpg';
  private readonly defaultUrl = 'https://merakisaude.com.br';

  /**
   * Update meta tags for the current page
   */
  updateMetaTags(tags: MetaTags): void {
    const title = tags.title ? `${tags.title} - Meraki Saúde Integrativa` : this.defaultTitle;
    const description = tags.description || this.defaultDescription;
    const image = tags.image || this.defaultImage;
    const url = tags.url || this.defaultUrl;
    const type = tags.type || 'website';

    // Update page title
    this.title.setTitle(title);

    // Update basic meta tags
    this.updateTag('name', 'description', description);
    if (tags.keywords) {
      this.updateTag('name', 'keywords', tags.keywords);
    }

    // Update Open Graph tags
    this.updateTag('property', 'og:title', title);
    this.updateTag('property', 'og:description', description);
    this.updateTag('property', 'og:image', image);
    this.updateTag('property', 'og:url', url);
    this.updateTag('property', 'og:type', type);

    // Update Twitter Card tags
    this.updateTag('name', 'twitter:card', 'summary_large_image');
    this.updateTag('name', 'twitter:title', title);
    this.updateTag('name', 'twitter:description', description);
    this.updateTag('name', 'twitter:image', image);
  }

  /**
   * Update a meta tag by name or property
   */
  private updateTag(attr: 'name' | 'property', selector: string, content: string): void {
    const existingTag = this.meta.getTag(`${attr}="${selector}"`);
    if (existingTag) {
      this.meta.updateTag({ [attr]: selector, content });
    } else {
      this.meta.addTag({ [attr]: selector, content });
    }
  }

  /**
   * Reset meta tags to default values
   */
  resetMetaTags(): void {
    this.updateMetaTags({
      title: this.defaultTitle,
      description: this.defaultDescription,
    });
  }
}

