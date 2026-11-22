import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  protected readonly auth = inject(AuthService);

  constructor() {
    // After a page refresh the token is still in storage, but the user profile is not.
    if (this.auth.isAuthenticated()) {
      // A 401 here is handled by the interceptor (it signs the user out).
      this.auth.loadCurrentUser().subscribe({ error: () => undefined });
    }
  }
}
