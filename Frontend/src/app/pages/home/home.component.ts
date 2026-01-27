import { Component, type OnInit, inject, signal, DestroyRef } from "@angular/core"
import { NgOptimizedImage } from "@angular/common"
import { takeUntilDestroyed } from "@angular/core/rxjs-interop"
import type { Therapist } from "../../models/therapist.model"
import type { Service } from "../../models/service.model"
import type { Testimonial } from "../../models/testimonial.model"
import { TherapistsService, ServiceManager, TestimonialsService } from "../../services"
import { MetaService } from "../../services/meta.service"
import { of } from "rxjs"
import { catchError } from "rxjs/operators"
import { FooterComponent } from "../../components/footer/footer.component"
import { TherapistsSectionComponent } from "../../components/therapists-section/therapists-section.component"
import { ServicesSectionComponent } from "../../components/services-section/services-section.component"
import { ContactFormComponent } from "../../components/contact-form/contact-form.component"
import { ServiceDialogComponent } from "../../components/service-dialog/service-dialog.component"
import { FaqSectionComponent } from "../../components/faq-section/faq-section.component"
import { WhatsappButtonComponent } from "../../components/whatsapp-button/whatsapp-button.component"
import { TestimonialsSectionComponent } from "../../components/testimonials-section/testimonials-section.component"

@Component({
  selector: "app-home",
  standalone: true,
  imports: [
    NgOptimizedImage,
    FooterComponent,
    TherapistsSectionComponent,
    ServicesSectionComponent,
    ContactFormComponent,
    ServiceDialogComponent,
    FaqSectionComponent,
    WhatsappButtonComponent,
    TestimonialsSectionComponent,
  ],
  templateUrl: "./home.component.html",
  styleUrl: "./home.component.css",
})
export class HomeComponent implements OnInit {
  private therapistsService: TherapistsService = inject(TherapistsService)
  private serviceManager: ServiceManager = inject(ServiceManager)
  private testimonialsService: TestimonialsService = inject(TestimonialsService)
  private metaService = inject(MetaService)
  private destroyRef = inject(DestroyRef)

  therapists = signal<Therapist[]>([])
  services = signal<Service[]>([])
  testimonials = signal<Testimonial[]>([])
  logoUrl = "https://hebbkx1anhila5yf.public.blob.vercel-storage.com/logo_white-xcKcT0DDe9L3p9gP0dexj9UyBLPudv.jpeg"
  isLoading = signal(true)

  ngOnInit(): void {
    // Update meta tags for home page
    this.metaService.updateMetaTags({
      title: 'Página Inicial',
      description: 'Meraki Saúde Integrativa oferece tratamentos terapêuticos personalizados com acupuntura, fisioterapia, massoterapia e psicoterapia. Agende sua consulta hoje!',
    });

    this.therapistsService
      .list()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        catchError(() => {
          this.therapists.set([])
          this.checkLoading()
          return of([] as Therapist[])
        })
      )
      .subscribe((therapists: Therapist[]) => {
        this.therapists.set(therapists)
        this.checkLoading()
      })

    this.serviceManager
      .list()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        catchError(() => {
          this.services.set([])
          this.checkLoading()
          return of([] as Service[])
        })
      )
      .subscribe((services: Service[]) => {
        this.services.set(services)
        this.checkLoading()
      })

    this.testimonialsService
      .list()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        catchError(() => {
          this.testimonials.set([])
          this.checkLoading()
          return of([] as Testimonial[])
        })
      )
      .subscribe((testimonials: Testimonial[]) => {
        this.testimonials.set(testimonials)
        this.checkLoading()
      })
  }

  private checkLoading(): void {
    // Set loading to false once all data has been loaded (even if empty due to errors)
    // We check that the signals have been initialized (not just empty arrays)
    const therapistsLoaded = this.therapists() !== undefined
    const servicesLoaded = this.services() !== undefined
    const testimonialsLoaded = this.testimonials() !== undefined
    
    if (therapistsLoaded && servicesLoaded && testimonialsLoaded) {
      this.isLoading.set(false)
    }
  }

  scrollToSection(sectionId: string): void {
    const element = document.getElementById(sectionId)
    if (element) {
      element.scrollIntoView({ 
        behavior: 'smooth', 
        block: 'start' 
      })
    }
  }
}
