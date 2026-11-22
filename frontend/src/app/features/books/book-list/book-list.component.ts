import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toErrorMessage } from '../../../core/errors/error-message';
import {
  Book,
  READING_STATUS_LABELS,
  READING_STATUSES,
  ReadingStatus,
} from '../../../core/models/book.models';
import { BooksService } from '../books.service';

@Component({
  selector: 'app-book-list',
  imports: [RouterLink],
  templateUrl: './book-list.component.html',
  styleUrl: './book-list.component.css',
})
export class BookListComponent implements OnInit {
  private readonly booksService = inject(BooksService);

  protected readonly statuses = READING_STATUSES;
  protected readonly labels = READING_STATUS_LABELS;

  protected readonly books = signal<Book[]>([]);
  protected readonly filter = signal<ReadingStatus | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected onFilterChange(value: string): void {
    this.filter.set(value ? (value as ReadingStatus) : null);
    this.load();
  }

  protected stars(rating: number): string {
    return '★'.repeat(rating) + '☆'.repeat(5 - rating);
  }

  protected remove(book: Book): void {
    if (!confirm(`Delete "${book.title}"?`)) {
      return;
    }

    this.booksService.delete(book.id).subscribe({
      next: () => this.books.update((books) => books.filter((b) => b.id !== book.id)),
      error: (err: unknown) => this.error.set(toErrorMessage(err)),
    });
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.booksService.list(this.filter()).subscribe({
      next: (books) => {
        this.books.set(books);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.error.set(toErrorMessage(err));
        this.loading.set(false);
      },
    });
  }
}
