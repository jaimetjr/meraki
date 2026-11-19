import { Component, input } from "@angular/core"
import { ImageOrInitialsComponent } from "../image-or-initials/image-or-initials.component"
import type { Therapist } from "../../models/therapist.model"

@Component({
  selector: "app-therapist-card",
  standalone: true,
  imports: [ImageOrInitialsComponent],
  templateUrl: "./therapist-card.component.html",
  styleUrl: "./therapist-card.component.css",
})
export class TherapistCardComponent {
  therapist = input.required<Therapist>()
}
