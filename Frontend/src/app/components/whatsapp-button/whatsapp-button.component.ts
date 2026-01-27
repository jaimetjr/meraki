import { Component } from "@angular/core"

@Component({
  selector: "app-whatsapp-button",
  standalone: true,
  templateUrl: "./whatsapp-button.component.html",
  styleUrl: "./whatsapp-button.component.css",
})
export class WhatsappButtonComponent {
  phoneNumber = "51994057757"
  message = "Olá! Gostaria de agendar uma consulta na Meraki Saúde Integrativa."

  openWhatsApp(): void {
    const url = `https://wa.me/${this.phoneNumber}?text=${encodeURIComponent(this.message)}`
    window.open(url, "_blank")
  }
}
