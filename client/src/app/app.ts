import { Component, inject, OnInit } from '@angular/core';
import { NgIf } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';
import { TodoApiService } from './core/todo-api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NgIf, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent implements OnInit {
  readonly auth = inject(AuthService); readonly router = inject(Router);
  private readonly api = inject(TodoApiService);
  ngOnInit(): void { if (this.auth.isAuthenticated) this.api.listSkins().subscribe({ next: skins => { const skin = skins.find(item => item.isActive); if (skin) { const root = document.documentElement; root.style.setProperty('--primary', skin.primaryColor); root.style.setProperty('--accent', skin.accentColor); root.style.setProperty('--canvas', skin.backgroundColor); root.style.setProperty('--app-font', skin.fontFamily); } }, error: () => undefined }); }
  logout(): void { this.auth.logout(); }
}
