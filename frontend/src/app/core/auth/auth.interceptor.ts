import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

const API_URLS = [environment.identityApiUrl, environment.booksApiUrl];

/**
 * Sends the Bearer token to our own APIs only (never to third-party URLs)
 * and ends the session when an authenticated call comes back 401 (expired or revoked token).
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.token();
  const isApiCall = API_URLS.some((url) => req.url.startsWith(url));

  const request = token && isApiCall
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401 && token && isApiCall) {
        auth.logout();
      }
      return throwError(() => error);
    }),
  );
};
