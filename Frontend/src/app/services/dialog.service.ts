import { Injectable, signal } from "@angular/core"
import type { Service } from "../models/service.model"

@Injectable({
  providedIn: "root",
})
export class DialogService {
  private isOpenSignal = signal<boolean>(false)
  private serviceSignal = signal<Service | null>(null)

  get isOpen$() {
    return this.isOpenSignal.asReadonly()
  }

  get service$() {
    return this.serviceSignal.asReadonly()
  }

  openDialog(service: Service): void {
    this.serviceSignal.set(service)
    this.isOpenSignal.set(true)
  }

  closeDialog(): void {
    this.isOpenSignal.set(false)
  }
}
