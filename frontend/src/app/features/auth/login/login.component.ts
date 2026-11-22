import { Component, inject, input, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { toErrorMessage } from '../../../core/errors/error-message';

export const DEMO_CREDENTIALS = { email: 'demo@readinglist.dev', password: 'Demo@123' } as const;

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  /** Bound from the ?returnUrl= query param set by the auth guard. */
  readonly returnUrl = input<string>();

  protected readonly demo = DEMO_CREDENTIALS;
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  protected useDemoAccount(): void {
    this.form.setValue({ email: this.demo.email, password: this.demo.password });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => void this.router.navigateByUrl(this.returnUrl() || '/books'),
      error: (err: unknown) => {
        this.error.set(toErrorMessage(err));
        this.submitting.set(false);
      },
    });
  }
}
