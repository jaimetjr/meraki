import { Component, input } from "@angular/core"
import { NgOptimizedImage } from "@angular/common"
import type { Therapist } from "../../models/therapist.model"

@Component({
  selector: "app-therapist-card",
  standalone: true,
  imports: [NgOptimizedImage],
  templateUrl: "./therapist-card.component.html",
  styleUrl: "./therapist-card.component.css",
})
export class TherapistCardComponent {
  therapist = input.required<Therapist>()
}
