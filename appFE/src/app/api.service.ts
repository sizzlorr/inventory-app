import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = '/api';

  login(username: string, password: string): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(
      `${this.baseUrl}/auth/login`,
      { username, password },
      { withCredentials: true }
    );
  }

  setToken(token: string) {
    localStorage.setItem('token', token);
  }

  getItems(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/inventory`);
  }

  getCategories(): Observable<{ id: number; name: string }[]> {
    return this.http.get<{ id: number; name: string }[]>(`${this.baseUrl}/categories`);
  }

  uploadImage(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>(`${this.baseUrl}/files/upload`, formData);
  }

  createItem(item: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/inventory`, item);
  }

  refreshToken() {
    return this.http.post<any>(`${this.baseUrl}/auth/refreshToken`, {}, { withCredentials: true });
  }

  saveToken(token: string) {
    localStorage.setItem('token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  logout() {
    localStorage.removeItem('token');
    // Також викликаємо бекенд, щоб вбити куку (бонус)
    this.http.post(`${this.baseUrl}/auth/logout`, {}, { withCredentials: true }).subscribe();
  }
}
