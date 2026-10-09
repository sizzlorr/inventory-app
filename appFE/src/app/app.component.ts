import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from './api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div style="padding: 20px; font-family: sans-serif; max-width: 800px; margin: 0 auto;">
      <h1>🛸 Gunpla & Hobbies Inventory</h1>

      <!-- СЕКЦІЯ ЛОГІНУ -->
      <div *ngIf="!isLoggedIn" style="background: #f0f0f0; padding: 15px; border-radius: 8px; margin-bottom: 20px;">
        <h3>Авторизація (Admin)</h3>
        <input [(ngModel)]="username" placeholder="Логін (admin)" style="margin-right: 10px;" />
        <input [(ngModel)]="password" type="password" placeholder="Пароль (admin123)" style="margin-right: 10px;" />
        <button (click)="onLogin()">Увійти</button>
      </div>

      <div *ngIf="isLoggedIn" style="background: #e6ffe6; padding: 10px; border-radius: 8px; margin-bottom: 20px;">
        ✅ Ви залогінені як Адмін!
      </div>

      <!-- СЕКЦІЯ ДОДАВАННЯ ТОВАРУ (Тільки для залогінених) -->
      <div *ngIf="isLoggedIn" style="border: 1px solid #ccc; padding: 15px; border-radius: 8px; margin-bottom: 20px;">
        <h3>Додати новий товар</h3>
        <input [(ngModel)]="newItem.name" placeholder="Назва товару" style="display: block; margin-bottom: 10px; width: 100%;" />
        <input [(ngModel)]="newItem.price" type="number" placeholder="Ціна" style="display: block; margin-bottom: 10px; width: 100%;" />
        <select [(ngModel)]="newItem.categoryId" style="display: block; margin-bottom: 10px; width: 100%; padding: 4px;">
          <option *ngFor="let cat of categories" [value]="cat.id">{{ cat.name }}</option>
        </select>

        <label>Фото товару:</label>
        <input type="file" (change)="onFileSelected($event)" style="display: block; margin-bottom: 10px;" />

        <button (click)="onCreateItem()">Зберегти товар</button>
      </div>

      <!-- КАТАЛОГ ТОВАРІВ -->
      <h2>Каталог</h2>
      <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 15px;">
        <div *ngFor="let item of items" style="border: 1px solid #ddd; padding: 10px; border-radius: 8px; text-align: center;">
          <img [src]="item.imageUrl ? 'http://localhost' + item.imageUrl : 'https://via.placeholder.com/150'"
               style="max-width: 100%; height: 150px; object-fit: cover; border-radius: 4px;" />
          <h4 style="margin: 10px 0 5px 0;">{{ item.name }}</h4>
          <p style="color: #666; font-weight: bold; margin: 0;">{{ item.price }} UAH</p>
          <small style="color: #888;">{{ item.categoryName }}</small>
        </div>
      </div>
    </div>
  `
})
export class AppComponent {
  private api = inject(ApiService);

  items: any[] = [];
  categories: { id: number; name: string }[] = [];
  isLoggedIn = false;

  // Форма логіну
  username = 'admin';
  password = 'admin123';

  // Форма товару
  newItem = {
    name: '',
    price: 0,
    isAvailable: true,
    categoryId: 1,
    imageUrl: ''
  };
  selectedFile: File | null = null;

  ngOnInit() {
    this.loadItems();
    this.loadCategories();
  }

  loadItems() {
    this.api.getItems().subscribe(data => this.items = data);
  }

  loadCategories() {
    this.api.getCategories().subscribe(data => {
      this.categories = data;
      if (data.length > 0) {
        this.newItem.categoryId = data[0].id;
      }
    });
  }

  onLogin() {
    this.api.login(this.username, this.password).subscribe({
      next: (res) => {
        this.api.setToken(res.token);
        this.isLoggedIn = true;
      },
      error: (err) => {
        console.error('Login failed', err);
        const serverMessage = err?.error?.message ?? err?.error?.Message;
        alert(serverMessage ? `Помилка логіну: ${serverMessage}` : 'Помилка логіну! Перевір чи backend запущений і CORS налаштований.');
      }
    });
  }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  onCreateItem() {
    if (this.selectedFile) {
      // Спочатку заливаємо фото, потім створюємо товар
      this.api.uploadImage(this.selectedFile).subscribe(res => {
        this.newItem.imageUrl = res.url;
        this.saveItem();
      });
    } else {
      this.saveItem();
    }
  }

  private saveItem() {
    this.api.createItem(this.newItem).subscribe({
      next: () => {
        alert('Товар успішно додано!');
        this.loadItems();
        this.newItem = { name: '', price: 0, isAvailable: true, categoryId: this.categories[0]?.id ?? 1, imageUrl: '' };
      },
      error: (err) => alert('Помилка при збереженні! Перевір валідацію або токен.')
    });
  }
}
