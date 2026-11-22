import { provideZonelessChangeDetection, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, provideRouter, RouterStateSnapshot, UrlTree } from '@angular/router';
import { authGuard, guestGuard } from './auth.guards';
import { AuthService } from './auth.service';

describe('auth guards', () => {
  const isAuthenticated = signal(false);
  const route = {} as ActivatedRouteSnapshot;
  const state = { url: '/books/new' } as RouterStateSnapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        { provide: AuthService, useValue: { isAuthenticated } },
      ],
    });
  });

  const run = (guard: typeof authGuard) => TestBed.runInInjectionContext(() => guard(route, state));

  it('authGuard lets signed-in users through', () => {
    isAuthenticated.set(true);
    expect(run(authGuard)).toBeTrue();
  });

  it('authGuard sends guests to login and remembers the page', () => {
    isAuthenticated.set(false);
    const result = run(authGuard) as UrlTree;
    expect(result.toString()).toBe('/login?returnUrl=%2Fbooks%2Fnew');
  });

  it('guestGuard sends signed-in users to their books', () => {
    isAuthenticated.set(true);
    expect((run(guestGuard) as UrlTree).toString()).toBe('/books');
  });

  it('guestGuard lets guests open login', () => {
    isAuthenticated.set(false);
    expect(run(guestGuard)).toBeTrue();
  });
});
