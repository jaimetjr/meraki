import { Component, ChangeDetectionStrategy } from "@angular/core"

@Component({
  selector: "app-footer",
  standalone: true,
  templateUrl: "./footer.component.html",
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: "./footer.component.css",
})
export class FooterComponent {
  logoUrl = "/logo-emblem.png"
  currentYear = new Date().getFullYear()
}
