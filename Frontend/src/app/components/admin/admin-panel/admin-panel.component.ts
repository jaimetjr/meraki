import { Component, input } from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "app-admin-panel",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./admin-panel.component.html",
  styleUrl: "./admin-panel.component.css",
})
export class AdminPanelComponent {
  title = input<string>("");
  description = input<string>("");
  actionsAlign = input<"start" | "end">("end");
}

