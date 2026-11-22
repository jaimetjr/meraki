import { Component, input } from "@angular/core";

@Component({
  selector: "app-admin-empty",
  standalone: true,
  templateUrl: "./admin-empty.component.html",
  styleUrl: "./admin-empty.component.css",
})
export class AdminEmptyComponent {
  message = input("Nenhum registro encontrado.");
}

