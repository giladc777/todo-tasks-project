import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TodoApiService } from '../core/todo-api.service';
import { Skin } from '../core/models';

@Component({
  selector: 'app-skins',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './skins.html',
  styleUrl: './skins.css'
})
export class SkinsComponent implements OnInit {
  private readonly api = inject(TodoApiService);
  skins: Skin[] = [];
  selected: Skin = this.createEmptySkin();
  error = '';

  ngOnInit(): void {
    this.api.listSkins().subscribe({
      next: skins => {
        this.skins = skins;
        this.selected = skins.find(skin => skin.isActive) ?? skins[0] ?? this.selected;

        if (this.selected.id) {
          this.apply(this.selected);
        }
      },
      error: () => {
        this.error = 'Could not load skins. Ensure Skins API is running.';
      }
    });
  }

  newSkin(): void {
    this.selected = {
      ...this.createEmptySkin(),
      name: 'My new skin'
    };
  }

  activate(skin: Skin): void {
    this.selected = { ...skin };

    this.api.activateSkin(skin.id).subscribe({
      next: activeSkin => {
        this.skins = this.skins.map(item => ({
          ...item,
          isActive: item.id === activeSkin.id
        }));
        this.selected = activeSkin;
        this.apply(activeSkin);
      },
      error: () => {
        this.error = 'Could not activate skin.';
      }
    });
  }

  save(): void {
    const request = {
      name: this.selected.name,
      primaryColor: this.selected.primaryColor,
      accentColor: this.selected.accentColor,
      backgroundColor: this.selected.backgroundColor,
      fontFamily: this.selected.fontFamily
    };
    const saveRequest = this.selected.id
      ? this.api.updateSkin(this.selected)
      : this.api.createSkin(request);

    saveRequest.subscribe({
      next: savedSkin => {
        this.selected = savedSkin;
        this.skins = this.skins.some(skin => skin.id === savedSkin.id)
          ? this.skins.map(skin => skin.id === savedSkin.id ? savedSkin : skin)
          : [...this.skins, savedSkin];
        this.apply(savedSkin);
      },
      error: () => {
        this.error = 'Could not save skin.';
      }
    });
  }

  private createEmptySkin(): Skin {
    return {
      id: '',
      name: '',
      primaryColor: '#176b87',
      accentColor: '#f4a261',
      backgroundColor: '#f2f7f8',
      fontFamily: 'Inter',
      isActive: false
    };
  }

  private apply(skin: Skin): void {
    const root = document.documentElement;
    root.style.setProperty('--primary', skin.primaryColor);
    root.style.setProperty('--accent', skin.accentColor);
    root.style.setProperty('--canvas', skin.backgroundColor);
    root.style.setProperty('--app-font', skin.fontFamily);
  }
}
