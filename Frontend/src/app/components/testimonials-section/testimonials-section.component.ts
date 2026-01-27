import { Component, input } from "@angular/core"
import type { Testimonial } from "../../models/testimonial.model"

@Component({
  selector: "app-testimonials-section",
  standalone: true,
  templateUrl: "./testimonials-section.component.html",
  styleUrl: "./testimonials-section.component.css",
})
export class TestimonialsSectionComponent {
  testimonials = input.required<Testimonial[]>()
  isLoading = input<boolean>(false)
}


