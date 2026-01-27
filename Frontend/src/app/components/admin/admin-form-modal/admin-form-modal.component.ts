import { Component, input, output } from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "app-admin-form-modal",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./admin-form-modal.component.html",
  styleUrl: "./admin-form-modal.component.css",
})
export class AdminFormModalComponent {
  title = input<string>("");
  visible = input<boolean>(false);
  close = output<void>();

  onBackdropClick(event: MouseEvent) {
    // Only close if clicking the backdrop itself, not the dialog content
    if (event.target === event.currentTarget) {
      this.close.emit();
    }
  }

  onCloseClick() {
    this.close.emit();
  }
}

