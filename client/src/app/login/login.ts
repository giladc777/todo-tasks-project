import { Component, inject } from '@angular/core';
import { NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, NgIf],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  username = 'admin';
  password = 'ChangeMe123!';
  error = '';
  loading = false;

  submit(): void {
    this.loading = true;
    this.error = '';

    this.auth.login({ username: this.username, password: this.password }).subscribe({
      next: () => {
        this.loading = false;
        void this.router.navigate(['/tasks']);
      },
      error: () => {
        this.loading = false;
        this.error = 'Sign-in failed. Check your credentials and ensure Users API is running.';
      }
    });
  }
}
