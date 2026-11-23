import type { MockedObject } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { UsersComponent } from './users.component';
import { UsersService } from '../../../services/users.service';
import { ToastService } from '../../../services/toast.service';
import { AuthService } from '../../../services/auth.service';
import { UserRole, type User } from '../../../models/user.model';

describe('UsersComponent (admin)', () => {
  let fixture: ComponentFixture<UsersComponent>;
  let component: UsersComponent;
  let usersService: MockedObject<Pick<UsersService, 'list' | 'create' | 'update' | 'delete' | 'updatePassword'>>;

  const inactive: User = { id: 'u1', email: 'admin@meraki.com', firstName: 'Admin', lastName: 'User', role: UserRole.Admin, isActive: false };
  const me: User = { id: 'me', email: 'superadmin@meraki.local', role: UserRole.SuperAdmin, isActive: true };

  beforeEach(async () => {
    usersService = {
      list: vi.fn().mockName('UsersService.list'),
      create: vi.fn().mockName('UsersService.create'),
      update: vi.fn().mockName('UsersService.update'),
      delete: vi.fn().mockName('UsersService.delete'),
      updatePassword: vi.fn().mockName('UsersService.updatePassword')
    };
    usersService.list.mockReturnValue(of([inactive, me]));
    usersService.update.mockReturnValue(of(void 0));
    usersService.create.mockReturnValue(of(inactive));
    usersService.updatePassword.mockReturnValue(of(void 0));
    const auth = {
      getCurrentUser: vi.fn().mockName('AuthService.getCurrentUser')
    };
    auth.getCurrentUser.mockReturnValue({ userId: 'me', email: me.email, role: 'SuperAdmin', token: 't', expiresAt: '2999-01-01' });

    await TestBed.configureTestingModule({
      imports: [UsersComponent],
      providers: [
        { provide: UsersService, useValue: usersService },
        { provide: AuthService, useValue: auth },
        { provide: ToastService, useValue: { showSuccess: vi.fn(), showError: vi.fn(), showWarning: vi.fn() } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UsersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  function chooseOption(controlName: string, label: string) {
    const select = fixture.nativeElement.querySelector(`select[formControlName=${controlName}]`) as HTMLSelectElement;
    const option = [...select.options].find((o) => o.textContent?.trim() === label)!;
    select.value = option.value;
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();
  }

  it('reactivates a user picked as "Ativo" in the Status dropdown, sending a real boolean', () => {
    component.startEdit(inactive);
    fixture.detectChanges();

    chooseOption('isActive', 'Ativo');
    component.submit();

    const sent = usersService.update.mock.lastCall![1];
    expect(sent.isActive).toBe(true);
  });

  it('refuses a 7-character password when creating a user (the API requires 8)', () => {
    component.startCreate();
    component.form.patchValue({ email: 'nova@meraki.com', password: 'abc1234' });

    component.submit();

    expect(usersService.create).not.toHaveBeenCalled();
  });

  it('requires Nome when creating a user', () => {
    component.startCreate();
    component.form.patchValue({ email: 'nova@meraki.com', password: 'abcd1234', firstName: '  ' });

    component.submit();

    expect(usersService.create).not.toHaveBeenCalled();
  });

  it('requires Nome when editing a user who has none yet', () => {
    component.startEdit(me);

    component.submit();

    expect(usersService.update).not.toHaveBeenCalled();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Nome é obrigatório.');
  });

  it('refuses a 7-character new password when editing a user (the API requires 8)', () => {
    component.startEdit(inactive);
    component.form.patchValue({ password: 'abc1234' });

    component.submit();

    expect(usersService.update).not.toHaveBeenCalled();
    expect(usersService.updatePassword).not.toHaveBeenCalled();
  });

  it('deactivates a user picked as "Inativo" in the Status dropdown, sending a real boolean', () => {
    component.startEdit({ ...inactive, isActive: true });
    fixture.detectChanges();

    chooseOption('isActive', 'Inativo');
    component.submit();

    const sent = usersService.update.mock.lastCall![1];
    expect(sent.isActive).toBe(false);
  });

  function select(controlName: string) {
    return fixture.nativeElement.querySelector(`select[formControlName=${controlName}]`) as HTMLSelectElement;
  }

  it('locks Papel and Status when you edit your own account, explains why, and sends neither', () => {
    component.startEdit(me);
    fixture.detectChanges();

    expect(select('role').disabled).toBe(true);
    expect(select('isActive').disabled).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Você não pode alterar seu próprio papel');

    component.form.patchValue({ firstName: 'Jaime' });
    component.submit();
    const sent = usersService.update.mock.lastCall![1];
    expect(sent.firstName).toBe('Jaime');
    expect('role' in sent).toBe(false);
    expect('isActive' in sent).toBe(false);
  });

  it('unlocks Papel and Status again when you then edit someone else or create a user', () => {
    component.startEdit(me);
    component.startEdit(inactive);
    fixture.detectChanges();
    expect(select('role').disabled).toBe(false);
    expect(select('isActive').disabled).toBe(false);

    component.startEdit(me);
    component.cancelEdit();
    component.startCreate();
    fixture.detectChanges();
    expect(select('role').disabled).toBe(false);
  });

  it('disables Excluir on your own row only', () => {
    const rows = [...fixture.nativeElement.querySelectorAll('.table-row')] as HTMLElement[];
    const deleteButton = (email: string) =>
      rows.find((r) => r.textContent?.includes(email))!.querySelector('.btn-danger') as HTMLButtonElement;

    expect(deleteButton(me.email).disabled).toBe(true);
    expect(deleteButton(inactive.email).disabled).toBe(false);
  });
});
