import { Component, input, output, ChangeDetectionStrategy } from "@angular/core";

@Component({
  selector: "app-admin-form-modal",
  standalone: true,
  templateUrl: "./admin-form-modal.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
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

