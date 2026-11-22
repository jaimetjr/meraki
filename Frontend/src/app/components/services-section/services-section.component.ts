import { Component, input, ChangeDetectionStrategy } from "@angular/core"
import type { Service } from "../../models/service.model"
import { ServiceCardComponent } from "../service-card/service-card.component"

@Component({
  selector: "app-services-section",
  standalone: true,
  imports: [ServiceCardComponent],
  templateUrl: "./services-section.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: "./services-section.component.css",
})
export class ServicesSectionComponent {
  services = input.required<Service[]>()
  isLoading = input<boolean>(false)
}


