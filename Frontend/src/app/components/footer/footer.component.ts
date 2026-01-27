import { Component } from "@angular/core"

@Component({
  selector: "app-footer",
  standalone: true,
  templateUrl: "./footer.component.html",
  styleUrl: "./footer.component.css",
})
export class FooterComponent {
  logoUrl = "https://hebbkx1anhila5yf.public.blob.vercel-storage.com/logo_white-xcKcT0DDe9L3p9gP0dexj9UyBLPudv.jpeg"
  currentYear = new Date().getFullYear()
}
