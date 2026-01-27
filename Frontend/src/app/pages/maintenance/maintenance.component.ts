import { Component } from "@angular/core"

@Component({
  selector: "app-maintenance",
  standalone: true,
  templateUrl: "./maintenance.component.html",
  styleUrl: "./maintenance.component.css",
})
export class MaintenanceComponent {
  logoUrl = "https://hebbkx1anhila5yf.public.blob.vercel-storage.com/logo_white-xcKcT0DDe9L3p9gP0dexj9UyBLPudv.jpeg" // Using the provided image for now

  contactInfo = {
    phones: [
      { number: "(51) 99405-7757", link: "tel:5199405-7757" },
      { number: "(51) 99730-3178", link: "tel:5199730-3178" },
    ],
    email: "merakisaudeintegrativa1@gmail.com",
    address: "Av. Senador Alberto Pasqualini, 714 - sala 201 - Lajeado, RS",
    hours: "Segunda a Sexta: 8h às 19h",
    instagram: "https://www.instagram.com/",
  }
}
