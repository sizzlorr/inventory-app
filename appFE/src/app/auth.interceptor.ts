// auth.interceptor.ts
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { ApiService } from './api.service'; // Шлях до твого сервісу

const AUTH_DEBUG = true;

function authLog(...args: unknown[]) {
  if (AUTH_DEBUG) {
    console.log('[auth-interceptor]', ...args);
  }
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(ApiService);
  const token = authService.getToken();
  const isAuthRequest = req.url.includes('/auth/login')
    || req.url.includes('/auth/refreshToken')
    || req.url.includes('/auth/logout');

  const attachTokenAndSend = (accessToken: string | null) => {
    let authReq = req;
    if (accessToken && !isAuthRequest) {
      authReq = req.clone({
        setHeaders: { Authorization: `Bearer ${accessToken}` }
      });
    }

    return next(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        authLog('Request failed', {
          url: req.url,
          method: req.method,
          status: error.status
        });

        // Якщо сервер відповів 401, і це НЕ запит на авторизацію чи рефреш
        if (error.status === 401 && !isAuthRequest) {
          authLog('401 received, attempting refresh token flow');
          return authService.refreshToken().pipe(
            switchMap((res: any) => {
              authService.saveToken(res.token);
              authLog('Refresh success after 401, retrying original request');

              const newReq = req.clone({
                setHeaders: { Authorization: `Bearer ${res.token}` }
              });
              return next(newReq);
            }),
            catchError((refreshError) => {
              authLog('Refresh failed after 401, logging out', refreshError);
              authService.logout();
              window.location.reload();
              return throwError(() => refreshError);
            })
          );
        }

        // Якщо це не 401 помилка, просто передаємо її далі
        return throwError(() => error);
      })
    );
  };

  authLog('Sending request with current auth state', {
    url: req.url,
    method: req.method,
    hasToken: !!token,
    isAuthRequest
  });

  return attachTokenAndSend(token);
};

