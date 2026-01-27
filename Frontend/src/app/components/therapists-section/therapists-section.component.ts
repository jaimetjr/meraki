import { Component, input } from "@angular/core"
import type { Therapist } from "../../models/therapist.model"
import { TherapistCardComponent } from "../therapist-card/therapist-card.component"

@Component({
  selector: "app-therapists-section",
  standalone: true,
  imports: [TherapistCardComponent],
  templateUrl: "./therapists-section.component.html",
  styleUrl: "./therapists-section.component.css",
})
export class TherapistsSectionComponent {
  therapists = input.required<Therapist[]>()
  isLoading = input<boolean>(false)
}



