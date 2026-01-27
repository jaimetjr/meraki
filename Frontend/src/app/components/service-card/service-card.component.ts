import { Component, input, inject } from "@angular/core"
import { NgOptimizedImage } from "@angular/common"
import type { Service } from "../../models/service.model"
import { DialogService } from "../../services/dialog.service"

@Component({
  selector: "app-service-card",
  standalone: true,
  imports: [NgOptimizedImage],
  templateUrl: "./service-card.component.html",
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
