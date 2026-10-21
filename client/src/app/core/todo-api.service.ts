import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Skin, TaskItem } from './models';

@Injectable({ providedIn: 'root' })
export class TodoApiService {
  private readonly http = inject(HttpClient);
  private readonly tasksUrl = 'http://localhost:5101/api/tasks';
  private readonly skinsUrl = 'http://localhost:5102/api/skins';

  listTasks(): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(this.tasksUrl);
  }

  createTask(value: { title: string; description?: string }): Observable<TaskItem> {
    return this.http.post<TaskItem>(this.tasksUrl, value);
  }

  updateTask(task: TaskItem): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${this.tasksUrl}/${task.id}`, task);
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<void>(`${this.tasksUrl}/${id}`);
  }

  listSkins(): Observable<Skin[]> {
    return this.http.get<Skin[]>(this.skinsUrl);
  }

  createSkin(skin: Omit<Skin, 'id' | 'isActive'>): Observable<Skin> {
    return this.http.post<Skin>(this.skinsUrl, skin);
  }

  updateSkin(skin: Skin): Observable<Skin> {
    return this.http.put<Skin>(`${this.skinsUrl}/${skin.id}`, skin);
  }

  activateSkin(id: string): Observable<Skin> {
    return this.http.put<Skin>(`${this.skinsUrl}/${id}/activate`, {});
  }
}
