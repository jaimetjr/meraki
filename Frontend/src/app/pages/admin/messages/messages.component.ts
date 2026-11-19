import { Component, OnInit, computed, inject, signal } from "@angular/core";
import { DatePipe } from "@angular/common";
import { AdminPanelComponent } from "../../../components/admin/admin-panel/admin-panel.component";
import { AdminEmptyComponent } from "../../../components/admin/admin-empty/admin-empty.component";
import { AdminLoadingComponent } from "../../../components/admin/admin-loading/admin-loading.component";
import { AdminConfirmDialogComponent } from "../../../components/admin/admin-confirm-dialog/admin-confirm-dialog.component";
import { ContactService } from "../../../services/contact.service";
import { ToastService } from "../../../services/toast.service";
import type { ContactMessage } from "../../../models/contact.model";

/** Admin inbox for messages sent through the public contact form. */
@Component({
  selector: "app-admin-messages",
  standalone: true,
  imports: [DatePipe, AdminPanelComponent, AdminEmptyComponent, AdminLoadingComponent, AdminConfirmDialogComponent],
  templateUrl: "./messages.component.html",
  styleUrl: "./messages.component.css",
})
export class MessagesComponent implements OnInit {
  private contactService = inject(ContactService);
  private toastService = inject(ToastService);

  messages = signal<ContactMessage[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  confirmId = signal<string | null>(null);
  unreadCount = computed(() => this.messages().filter((m) => !m.isRead).length);

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set(null);
    this.contactService.list().subscribe({
      next: (data) => this.messages.set(data),
      error: () => {
        this.error.set("Não foi possível carregar as mensagens");
        this.loading.set(false);
      },
      complete: () => this.loading.set(false),
    });
  }

  toggleRead(message: ContactMessage) {
    const isRead = !message.isRead;
    this.contactService.markRead(message.id, isRead).subscribe({
      next: () => this.messages.update((list) => list.map((m) => (m.id === message.id ? { ...m, isRead } : m))),
      error: () => this.toastService.showError("Não foi possível atualizar a mensagem"),
    });
  }

  confirmDelete(id: string) {
    this.confirmId.set(id);
  }

  cancelDelete() {
    this.confirmId.set(null);
  }

  delete() {
    const id = this.confirmId();
    if (!id) return;
    this.contactService.delete(id).subscribe({
      next: () => {
        this.messages.update((list) => list.filter((m) => m.id !== id));
        this.toastService.showSuccess("Mensagem excluída");
        this.confirmId.set(null);
      },
      error: () => this.toastService.showError("Não foi possível excluir a mensagem"),
    });
  }
}
