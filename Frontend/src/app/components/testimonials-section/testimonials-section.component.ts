import { Component, input, ChangeDetectionStrategy } from "@angular/core"
import { ImageOrInitialsComponent } from "../image-or-initials/image-or-initials.component"
import type { Testimonial } from "../../models/testimonial.model"

@Component({
  selector: "app-testimonials-section",
  standalone: true,
  imports: [ImageOrInitialsComponent],
  templateUrl: "./testimonials-section.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: "./testimonials-section.component.css",
})
export class TestimonialsSectionComponent {
  testimonials = input.required<Testimonial[]>()
  isLoading = input<boolean>(false)
}


