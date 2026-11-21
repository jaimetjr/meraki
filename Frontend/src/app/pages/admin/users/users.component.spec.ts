import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { UsersComponent } from './users.component';
import { UsersService } from '../../../services/users.service';
import { ToastService } from '../../../services/toast.service';
import { UserRole, type User } from '../../../models/user.model';

describe('UsersComponent (admin)', () => {
  let fixture: ComponentFixture<UsersComponent>;
  let component: UsersComponent;
  let usersService: jasmine.SpyObj<UsersService>;

  const inactive: User = { id: 'u1', email: 'admin@meraki.com', firstName: 'Admin', lastName: 'User', role: UserRole.Admin, isActive: false };

  beforeEach(async () => {
    usersService = jasmine.createSpyObj('UsersService', ['list', 'create', 'update', 'delete', 'updatePassword']);
    usersService.list.and.returnValue(of([inactive]));
    usersService.update.and.returnValue(of(void 0));
    usersService.create.and.returnValue(of(inactive));
    usersService.updatePassword.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [UsersComponent],
      providers: [
        { provide: UsersService, useValue: usersService },
        { provide: ToastService, useValue: jasmine.createSpyObj('ToastService', ['showSuccess', 'showError', 'showWarning']) },
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

    const sent = usersService.update.calls.mostRecent().args[1];
    expect(sent.isActive).toBe(true);
  });

  it('refuses a 7-character password when creating a user (the API requires 8)', () => {
    component.startCreate();
    component.form.patchValue({ email: 'nova@meraki.com', password: 'abc1234' });

    component.submit();

    expect(usersService.create).not.toHaveBeenCalled();
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

    const sent = usersService.update.calls.mostRecent().args[1];
    expect(sent.isActive).toBe(false);
  });
});
