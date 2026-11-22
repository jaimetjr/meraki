import { Component, input, output, ChangeDetectionStrategy } from "@angular/core";

@Component({
  selector: "app-admin-form-actions",
  standalone: true,
  templateUrl: "./admin-form-actions.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: "./admin-form-actions.component.css",
})
export class AdminFormActionsComponent {
  saveLabel = input("Salvar");
  cancelLabel = input("Cancelar");
  disableSave = input(false);
  cancel = output<void>();
}

