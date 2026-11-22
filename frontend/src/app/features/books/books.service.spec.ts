import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { BooksService } from './books.service';

describe('BooksService', () => {
  const url = `${environment.booksApiUrl}/books`;
  let service: BooksService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(BooksService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('lists all books without a filter', () => {
    service.list().subscribe();
    const req = http.expectOne(url);
    expect(req.request.params.has('status')).toBeFalse();
    req.flush([]);
  });

  it('sends the status filter as a query param', () => {
    service.list('Read').subscribe();
    http.expectOne(`${url}?status=Read`).flush([]);
  });

  it('creates with title and author only', () => {
    service.create({ title: 'Clean Code', author: 'Robert C. Martin' }).subscribe();
    const req = http.expectOne(url);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ title: 'Clean Code', author: 'Robert C. Martin' });
    req.flush({});
  });

  it('updates and deletes by id', () => {
    service.update('42', { title: 'T', author: 'A', status: 'Read', rating: 5 }).subscribe();
    service.delete('42').subscribe();

    expect(http.expectOne({ method: 'PUT', url: `${url}/42` }).request.body.rating).toBe(5);
    http.expectOne({ method: 'DELETE', url: `${url}/42` }).flush(null);
    http.match(`${url}/42`).forEach((r) => r.flush({}));
  });
});
