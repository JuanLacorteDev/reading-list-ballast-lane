import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { User } from '../models/auth.models';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  const user: User = { id: '1', name: 'Reader', email: 'reader@example.com', createdAt: '2026-01-01T00:00:00Z' };
  const inOneHour = () => new Date(Date.now() + 3_600_000).toISOString();

  let http: HttpTestingController;

  function createService(): AuthService {
    return TestBed.inject(AuthService);
  }

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('starts signed out when storage is empty', () => {
    expect(createService().isAuthenticated()).toBeFalse();
  });

  it('login stores the token and loads the current user', () => {
    const service = createService();
    let result: User | undefined;

    service.login({ email: user.email, password: 'secret123' }).subscribe((u) => (result = u));

    http.expectOne(`${environment.identityApiUrl}/auth/login`).flush({ accessToken: 'jwt', expiresAt: inOneHour() });
    http.expectOne(`${environment.identityApiUrl}/auth/me`).flush(user);

    expect(result).toEqual(user);
    expect(service.isAuthenticated()).toBeTrue();
    expect(service.token()).toBe('jwt');
    expect(service.currentUser()).toEqual(user);
    expect(localStorage.getItem('reading-list.token')).toBe('jwt');
  });

  it('treats an expired token as signed out', () => {
    localStorage.setItem('reading-list.token', 'old-jwt');
    localStorage.setItem('reading-list.expiresAt', new Date(Date.now() - 1000).toISOString());

    expect(createService().isAuthenticated()).toBeFalse();
  });

  it('restores a valid session from storage', () => {
    localStorage.setItem('reading-list.token', 'jwt');
    localStorage.setItem('reading-list.expiresAt', inOneHour());

    expect(createService().isAuthenticated()).toBeTrue();
  });

  it('logout clears the session and goes to the login page', () => {
    localStorage.setItem('reading-list.token', 'jwt');
    localStorage.setItem('reading-list.expiresAt', inOneHour());
    const service = createService();
    const navigate = spyOn(TestBed.inject(Router), 'navigate').and.resolveTo(true);

    service.logout();

    expect(service.isAuthenticated()).toBeFalse();
    expect(service.currentUser()).toBeNull();
    expect(localStorage.getItem('reading-list.token')).toBeNull();
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });
});
