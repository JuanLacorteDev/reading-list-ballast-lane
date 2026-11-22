import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Book, CreateBookRequest, ReadingStatus, UpdateBookRequest } from '../../core/models/book.models';

@Injectable({ providedIn: 'root' })
export class BooksService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.booksApiUrl}/books`;

  list(status?: ReadingStatus | null): Observable<Book[]> {
    const params = status ? new HttpParams().set('status', status) : undefined;
    return this.http.get<Book[]>(this.baseUrl, { params });
  }

  get(id: string): Observable<Book> {
    return this.http.get<Book>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateBookRequest): Observable<Book> {
    return this.http.post<Book>(this.baseUrl, request);
  }

  update(id: string, request: UpdateBookRequest): Observable<Book> {
    return this.http.put<Book>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
