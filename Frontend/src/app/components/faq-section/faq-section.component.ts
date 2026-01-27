import { Component } from "@angular/core"

interface FAQItem {
  question: string
  answer: string
}

@Component({
  selector: "app-faq-section",
  standalone: true,
  templateUrl: "./faq-section.component.html",
  styleUrl: "./faq-section.component.css",
})
export class FaqSectionComponent {
  faqs: FAQItem[] = [
    {
      question: "Como agendar uma consulta?",
      answer: "Você pode agendar uma consulta através do nosso formulário de contato, por telefone ou WhatsApp. Nossa equipe entrará em contato para confirmar o horário e fornecer todas as informações necessárias."
    },
    {
      question: "Quais são os métodos de pagamento aceitos?",
      answer: "Aceitamos dinheiro, cartões de crédito e débito, PIX e transferência bancária. Também oferecemos planos de parcelamento para tratamentos mais longos."
    },
    {
      question: "Os tratamentos são cobertos por planos de saúde?",
      answer: "Alguns tratamentos podem ser cobertos por planos de saúde, dependendo da sua cobertura. Recomendamos verificar com seu plano de saúde antes do início do tratamento."
    },
    {
      question: "Qual a duração de cada sessão?",
      answer: "A duração varia de acordo com o tratamento escolhido. Sessões de acupuntura duram em média 50 minutos, fisioterapia 60 minutos, massoterapia 60-90 minutos e psicoterapia 50 minutos."
    },
    {
      question: "Preciso de encaminhamento médico?",
      answer: "Para a maioria dos tratamentos não é necessário encaminhamento médico. No entanto, recomendamos consultar seu médico de confiança antes de iniciar qualquer tratamento, especialmente se você tiver condições médicas específicas."
    },
    {
      question: "Vocês atendem crianças?",
      answer: "Sim, atendemos crianças a partir de 5 anos de idade, com tratamentos adaptados para cada faixa etária. Para crianças menores, recomendamos uma avaliação prévia."
    },
    {
      question: "Qual a frequência recomendada para os tratamentos?",
      answer: "A frequência varia de acordo com o tratamento e condição individual. Geralmente, recomendamos sessões semanais no início, podendo ser reduzidas conforme a melhora do paciente."
    },
    {
      question: "Vocês fazem atendimento domiciliar?",
      answer: "Sim, oferecemos atendimento domiciliar para alguns tratamentos, especialmente para pacientes com dificuldade de locomoção. Entre em contato para verificar disponibilidade."
    }
  ]

  expandedItems: Set<number> = new Set()

  toggleItem(index: number): void {
    if (this.expandedItems.has(index)) {
      this.expandedItems.delete(index)
    } else {
      this.expandedItems.add(index)
    }
  }

  isExpanded(index: number): boolean {
    return this.expandedItems.has(index)
  }
}
