import { Component, input, output, ChangeDetectionStrategy } from "@angular/core";

@Component({
  selector: "app-admin-confirm-dialog",
  standalone: true,
  templateUrl: "./admin-confirm-dialog.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: "./admin-confirm-dialog.component.css",
})
export class AdminConfirmDialogComponent {
  title = input("Confirmar ação");
  message = input("Deseja continuar?");
  confirmLabel = input("Confirmar");
  cancelLabel = input("Cancelar");

  confirm = output<void>();
  cancel = output<void>();
}

