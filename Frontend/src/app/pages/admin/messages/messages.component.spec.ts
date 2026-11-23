import type { MockedObject } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { MessagesComponent } from './messages.component';
import { ContactService } from '../../../services/contact.service';
import { ToastService } from '../../../services/toast.service';
import type { ContactMessage } from '../../../models/contact.model';

describe('MessagesComponent (admin inbox)', () => {
  let fixture: ComponentFixture<MessagesComponent>;
  let component: MessagesComponent;
  let contactService: MockedObject<Pick<ContactService, 'list' | 'markRead' | 'delete'>>;

  const messages: ContactMessage[] = [
    { id: 'a', name: 'Ana', email: 'ana@example.com', phone: '1', service: 'Terapia', message: 'Oi', isRead: false, createdAt: '2026-11-20T10:00:00Z' },
    { id: 'b', name: 'Bruno', email: 'bruno@example.com', phone: '2', service: 'Outro', isRead: true, createdAt: '2026-11-19T10:00:00Z' },
  ];

  beforeEach(async () => {
    contactService = {
      list: vi.fn().mockName('ContactService.list'),
      markRead: vi.fn().mockName('ContactService.markRead'),
      delete: vi.fn().mockName('ContactService.delete')
    };
    contactService.list.mockReturnValue(of(structuredClone(messages)));
    contactService.markRead.mockReturnValue(of(void 0));
    contactService.delete.mockReturnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [MessagesComponent],
      providers: [
        { provide: ContactService, useValue: contactService },
        { provide: ToastService, useValue: { showSuccess: vi.fn(), showError: vi.fn() } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MessagesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('shows every message and counts the unread ones', () => {
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Ana');
    expect(text).toContain('Bruno');
    expect(component.unreadCount()).toBe(1);
  });

  it('toggles a message between read and unread', () => {
    component.toggleRead(component.messages()[0]);

    expect(contactService.markRead).toHaveBeenCalledWith('a', true);
    expect(component.messages()[0].isRead).toBe(true);
    expect(component.unreadCount()).toBe(0);
  });

  it('deletes a message only after confirmation', () => {
    component.confirmDelete('b');
    expect(contactService.delete).not.toHaveBeenCalled();

    component.delete();

    expect(contactService.delete).toHaveBeenCalledWith('b');
    expect(component.messages().map((m) => m.id)).toEqual(['a']);
  });
});
