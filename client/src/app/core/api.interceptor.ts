import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const apiInterceptor: HttpInterceptorFn = (request, next) => {
  const token = sessionStorage.getItem('todo_access_token');

  if (!token || request.url.endsWith('/auth/login')) {
    return next(request);
  }

  const router = inject(Router);
  const authenticatedRequest = request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authenticatedRequest).pipe(
    catchError(error => {
      if (error.status === 401) {
        sessionStorage.clear();
        void router.navigate(['/login']);
      }

      return throwError(() => error);
    })
  );
};
