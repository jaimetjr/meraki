import { Component, input, output } from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "app-admin-confirm-dialog",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./admin-confirm-dialog.component.html",
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

