import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  const token = signal<string | null>('jwt');
  const authStub = { token, logout: jasmine.createSpy('logout') };

  let http: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    token.set('jwt');
    authStub.logout.calls.reset();
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authStub },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('adds the bearer token to our APIs', () => {
    http.get(`${environment.booksApiUrl}/books`).subscribe();

    const req = httpMock.expectOne(`${environment.booksApiUrl}/books`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer jwt');
    req.flush([]);
  });

  it('never sends the token to other hosts', () => {
    http.get('https://example.com/data').subscribe();

    const req = httpMock.expectOne('https://example.com/data');
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush({});
  });

  it('sends no header when signed out', () => {
    token.set(null);
    http.get(`${environment.booksApiUrl}/books`).subscribe();

    const req = httpMock.expectOne(`${environment.booksApiUrl}/books`);
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush([]);
  });

  it('signs out when an authenticated call returns 401', () => {
    http.get(`${environment.booksApiUrl}/books`).subscribe({ error: () => undefined });

    httpMock.expectOne(`${environment.booksApiUrl}/books`).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(authStub.logout).toHaveBeenCalled();
  });

  it('does not sign out on other errors', () => {
    http.get(`${environment.booksApiUrl}/books/1`).subscribe({ error: () => undefined });

    httpMock.expectOne(`${environment.booksApiUrl}/books/1`).flush(null, { status: 404, statusText: 'Not Found' });

    expect(authStub.logout).not.toHaveBeenCalled();
  });
});
