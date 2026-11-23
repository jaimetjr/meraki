import { Component, input, computed, ChangeDetectionStrategy } from "@angular/core"
import { ImageOrInitialsComponent } from "../image-or-initials/image-or-initials.component"
import type { Therapist } from "../../models/therapist.model"

@Component({
  selector: "app-therapist-card",
  standalone: true,
  imports: [ImageOrInitialsComponent],
  templateUrl: "./therapist-card.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: "./therapist-card.component.css",
})
export class TherapistCardComponent {
  therapist = input.required<Therapist>()

  /** The services this therapist offers, e.g. "Reiki · Massagem"; "" when none are linked. */
  servicesLine = computed(() => (this.therapist().services ?? []).map((s) => s.name).join(" · "))
}
