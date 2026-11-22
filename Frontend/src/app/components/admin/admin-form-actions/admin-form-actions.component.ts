import { Component, input, output } from "@angular/core";

@Component({
  selector: "app-admin-form-actions",
  standalone: true,
  templateUrl: "./admin-form-actions.component.html",
  styleUrl: "./admin-form-actions.component.css",
})
export class AdminFormActionsComponent {
  saveLabel = input("Salvar");
  cancelLabel = input("Cancelar");
  disableSave = input(false);
  cancel = output<void>();
}

