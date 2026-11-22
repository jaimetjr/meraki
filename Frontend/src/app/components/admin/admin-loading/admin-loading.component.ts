import { Component, input, ChangeDetectionStrategy } from "@angular/core";

@Component({
  selector: "app-admin-loading",
  standalone: true,
  templateUrl: "./admin-loading.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: "./admin-loading.component.css",
})
export class AdminLoadingComponent {
  label = input("Carregando...");
}

