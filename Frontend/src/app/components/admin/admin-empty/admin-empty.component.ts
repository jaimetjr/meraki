import { Component, input, ChangeDetectionStrategy } from "@angular/core";

@Component({
  selector: "app-admin-empty",
  standalone: true,
  templateUrl: "./admin-empty.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: "./admin-empty.component.css",
})
export class AdminEmptyComponent {
  message = input("Nenhum registro encontrado.");
}

