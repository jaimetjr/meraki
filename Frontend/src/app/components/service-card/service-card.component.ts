import { Component, input, inject, ChangeDetectionStrategy } from "@angular/core"
import { ImageOrInitialsComponent } from "../image-or-initials/image-or-initials.component"
import type { Service } from "../../models/service.model"
import { DialogService } from "../../services/dialog.service"

@Component({
  selector: "app-service-card",
  standalone: true,
  imports: [ImageOrInitialsComponent],
  templateUrl: "./service-card.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: "./service-card.component.css",
})
export class ServiceCardComponent {
  service = input.required<Service>()
  private dialogService = inject(DialogService)

  openDialog(): void {
    const fullService: Service = {
      ...this.service(),
      longDescription:
        this.service().longDescription ||
        `${this.service().description}`,
      benefits: this.service().benefits,
      duration: this.service().duration,
      price: this.service().price,
    }

    this.dialogService.openDialog(fullService)
  }
}
