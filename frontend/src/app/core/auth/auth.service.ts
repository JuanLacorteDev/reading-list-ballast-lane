import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, switchMap, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/auth.models';

const TOKEN_KEY = 'reading-list.token';
const EXPIRES_AT_KEY = 'reading-list.expiresAt';

/**
 * Holds the session as signals. The token lives in localStorage so a refresh keeps the user signed in.
 * Trade-off: localStorage is readable by any script on the page (XSS); an HttpOnly cookie would be safer
 * but needs the API to issue cookies and handle CSRF.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = `${environment.identityApiUrl}/auth`;

  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly _expiresAt = signal<string | null>(localStorage.getItem(EXPIRES_AT_KEY));
  private readonly _currentUser = signal<User | null>(null);

  readonly token = this._token.asReadonly();
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => {
    const token = this._token();
    const expiresAt = this._expiresAt();
    return !!token && !!expiresAt && new Date(expiresAt).getTime() > Date.now();
  });

  login(request: LoginRequest): Observable<User> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, request).pipe(
      tap((response) => this.storeSession(response)),
      switchMap(() => this.loadCurrentUser()),
    );
  }

  register(request: RegisterRequest): Observable<User> {
    return this.http.post<User>(`${this.baseUrl}/register`, request);
  }

  loadCurrentUser(): Observable<User> {
    return this.http
      .get<User>(`${this.baseUrl}/me`)
      .pipe(tap((user) => this._currentUser.set(user)));
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EXPIRES_AT_KEY);
    this._token.set(null);
    this._expiresAt.set(null);
    this._currentUser.set(null);
    void this.router.navigate(['/login']);
  }

  private storeSession(response: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, response.accessToken);
    localStorage.setItem(EXPIRES_AT_KEY, response.expiresAt);
    this._token.set(response.accessToken);
    this._expiresAt.set(response.expiresAt);
  }
}
