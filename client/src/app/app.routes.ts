import { Routes } from '@angular/router';
import { LoginComponent } from './login/login';
import { TasksComponent } from './tasks/tasks';
import { SkinsComponent } from './skins/skins';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
	{ path: 'login', component: LoginComponent },
	{ path: 'tasks', component: TasksComponent, canActivate: [authGuard] },
	{ path: 'skins', component: SkinsComponent, canActivate: [authGuard] },
	{ path: '', pathMatch: 'full', redirectTo: 'tasks' },
	{ path: '**', redirectTo: 'tasks' }
];
