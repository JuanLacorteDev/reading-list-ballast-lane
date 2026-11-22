import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { toErrorMessage } from '../../../core/errors/error-message';
import {
  Book,
  READING_STATUS_LABELS,
  READING_STATUSES,
  ReadingStatus,
} from '../../../core/models/book.models';
import { BooksService } from '../books.service';

/**
 * Create and edit in one component. On create only title and author are sent:
 * a new book always starts as "Want to read" without a rating (domain rule).
 */
@Component({
  selector: 'app-book-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './book-form.component.html',
  styleUrl: './book-form.component.css',
})
export class BookFormComponent implements OnInit {
  private readonly booksService = inject(BooksService);
  private readonly router = inject(Router);

  /** Bound from the :id route param; undefined on /books/new. */
  readonly id = input<string>();

  protected readonly isEdit = computed(() => !!this.id());
  protected readonly statuses = READING_STATUSES;
  protected readonly labels = READING_STATUS_LABELS;
  protected readonly ratings = [1, 2, 3, 4, 5];

  protected readonly loading = signal(false);
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    author: ['', [Validators.required, Validators.maxLength(100)]],
    status: ['WantToRead' as ReadingStatus],
    rating: [null as number | null],
  });

  private readonly status = toSignal(this.form.controls.status.valueChanges, {
    initialValue: this.form.controls.status.value,
  });

  /** Rating only makes sense for books that were read. */
  protected readonly canRate = computed(() => this.status() === 'Read');

  ngOnInit(): void {
    const id = this.id();
    if (!id) {
      return;
    }

    this.loading.set(true);
    this.booksService.get(id).subscribe({
      next: (book) => {
        this.form.setValue({
          title: book.title,
          author: book.author,
          status: book.status,
          rating: book.rating,
        });
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.error.set(toErrorMessage(err));
        this.loading.set(false);
      },
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    this.save().subscribe({
      next: () => void this.router.navigate(['/books']),
      error: (err: unknown) => {
        this.error.set(toErrorMessage(err));
        this.submitting.set(false);
      },
    });
  }

  private save(): Observable<Book> {
    const { title, author, status, rating } = this.form.getRawValue();
    const id = this.id();

    if (!id) {
      return this.booksService.create({ title, author });
    }

    return this.booksService.update(id, {
      title,
      author,
      status,
      rating: status === 'Read' && rating ? Number(rating) : null,
    });
  }
}
