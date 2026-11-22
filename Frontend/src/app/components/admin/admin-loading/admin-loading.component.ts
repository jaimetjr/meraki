import { Component, input } from "@angular/core";

@Component({
  selector: "app-admin-loading",
  standalone: true,
  templateUrl: "./admin-loading.component.html",
  styleUrl: "./admin-loading.component.css",
})
export class AdminLoadingComponent {
  label = input("Carregando...");
}

