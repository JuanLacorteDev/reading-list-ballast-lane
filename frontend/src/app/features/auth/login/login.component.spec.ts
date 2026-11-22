import { provideZonelessChangeDetection } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/auth/auth.service';
import { DEMO_CREDENTIALS, LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let auth: jasmine.SpyObj<AuthService>;
  let fixture: ComponentFixture<LoginComponent>;
  let navigate: jasmine.Spy;

  beforeEach(async () => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['login']);
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [provideZonelessChangeDetection(), provideRouter([]), { provide: AuthService, useValue: auth }],
    }).compileComponents();
    navigate = spyOn(TestBed.inject(Router), 'navigateByUrl').and.resolveTo(true);
    fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
  });

  const el = () => fixture.nativeElement as HTMLElement;
  const clickDemo = () => el().querySelector<HTMLButtonElement>('[data-testid=demo-hint] button')!.click();
  const submit = async () => {
    el().querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    await fixture.whenStable();
  };

  it('shows the demo credentials', () => {
    expect(el().textContent).toContain(DEMO_CREDENTIALS.email);
  });

  it('signs in with the demo account and goes to the books', async () => {
    auth.login.and.returnValue(of({ id: '1', name: 'Demo', email: DEMO_CREDENTIALS.email, createdAt: '' }));

    clickDemo();
    await submit();

    expect(auth.login).toHaveBeenCalledWith({ ...DEMO_CREDENTIALS });
    expect(navigate).toHaveBeenCalledWith('/books');
  });

  it('shows the API error when sign in fails', async () => {
    auth.login.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 401, error: { detail: 'Invalid email or password.' } })),
    );

    clickDemo();
    await submit();

    expect(el().querySelector('[role=alert]')?.textContent).toContain('Invalid email or password.');
  });
});
