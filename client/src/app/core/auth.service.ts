import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = 'http://localhost:5103/api';

  get token(): string | null {
    return sessionStorage.getItem('todo_access_token');
  }

  get username(): string {
    return sessionStorage.getItem('todo_username') ?? '';
  }

  get isAuthenticated(): boolean {
    return !!this.token;
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, request).pipe(
      tap(result => {
        sessionStorage.setItem('todo_access_token', result.token);
        sessionStorage.setItem('todo_username', result.username);
        sessionStorage.setItem('todo_permissions', JSON.stringify(result.permissions));
      })
    );
  }

  logout(): void {
    sessionStorage.clear();
    void this.router.navigate(['/login']);
  }
}
