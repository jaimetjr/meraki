import { Component, input, ChangeDetectionStrategy } from "@angular/core";

@Component({
  selector: "app-admin-panel",
  standalone: true,
  templateUrl: "./admin-panel.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: "./admin-panel.component.css",
})
export class AdminPanelComponent {
  title = input<string>("");
  description = input<string>("");
  actionsAlign = input<"start" | "end">("end");
}

