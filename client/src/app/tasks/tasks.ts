import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TodoApiService } from '../core/todo-api.service';
import { TaskItem } from '../core/models';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './tasks.html',
  styleUrl: './tasks.css'
})
export class TasksComponent implements OnInit {
  private readonly api = inject(TodoApiService);
  tasks: TaskItem[] = [];
  newTitle = '';
  filter: 'all' | 'open' | 'done' = 'all';
  error = '';

  readonly today = new Intl.DateTimeFormat('en', {
    weekday: 'long',
    month: 'long',
    day: 'numeric'
  }).format(new Date());

  get completed(): number {
    return this.tasks.filter(task => task.isCompleted).length;
  }

  get remaining(): number {
    return this.tasks.length - this.completed;
  }

  get progress(): number {
    return this.tasks.length ? (this.completed / this.tasks.length) * 100 : 0;
  }

  get filteredTasks(): TaskItem[] {
    return this.tasks.filter(task => {
      if (this.filter === 'all') {
        return true;
      }

      return this.filter === 'done' ? task.isCompleted : !task.isCompleted;
    });
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.listTasks().subscribe({
      next: tasks => {
        this.tasks = tasks;
        this.error = '';
      },
      error: () => {
        this.error = 'Could not load tasks. Ensure Tasks API and Redis are running.';
      }
    });
  }

  addTask(): void {
    const title = this.newTitle.trim();
    if (!title) {
      return;
    }

    this.api.createTask({ title }).subscribe({
      next: task => {
        this.tasks = [task, ...this.tasks];
        this.newTitle = '';
        this.error = '';
      },
      error: () => {
        this.error = 'Could not add task. Please try again.';
      }
    });
  }

  toggle(task: TaskItem): void {
    const updatedTask = { ...task, isCompleted: !task.isCompleted };

    this.api.updateTask(updatedTask).subscribe({
      next: savedTask => {
        this.tasks = this.tasks.map(item => item.id === savedTask.id ? savedTask : item);
      },
      error: () => {
        this.error = 'Could not update task.';
      }
    });
  }

  remove(task: TaskItem): void {
    this.api.deleteTask(task.id).subscribe({
      next: () => {
        this.tasks = this.tasks.filter(item => item.id !== task.id);
      },
      error: () => {
        this.error = 'Could not delete task.';
      }
    });
  }
}
