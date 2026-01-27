import { Component, input } from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "app-admin-loading",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./admin-loading.component.html",
  styleUrl: "./admin-loading.component.css",
})
export class AdminLoadingComponent {
  label = input("Carregando...");
}

