import { Component, input } from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "app-admin-empty",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./admin-empty.component.html",
  styleUrl: "./admin-empty.component.css",
})
export class AdminEmptyComponent {
  message = input("Nenhum registro encontrado.");
}

