import { provideZonelessChangeDetection } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { Book } from '../../../core/models/book.models';
import { BooksService } from '../books.service';
import { BookFormComponent } from './book-form.component';

describe('BookFormComponent', () => {
  const book: Book = {
    id: '42',
    title: 'Clean Code',
    author: 'Robert C. Martin',
    status: 'Reading',
    rating: null,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
  };

  let booksService: jasmine.SpyObj<BooksService>;
  let fixture: ComponentFixture<BookFormComponent>;

  async function setup(id?: string) {
    booksService = jasmine.createSpyObj<BooksService>('BooksService', ['get', 'create', 'update']);
    booksService.get.and.returnValue(of(book));
    booksService.create.and.returnValue(of(book));
    booksService.update.and.returnValue(of(book));

    await TestBed.configureTestingModule({
      imports: [BookFormComponent],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        { provide: BooksService, useValue: booksService },
      ],
    }).compileComponents();

    spyOn(TestBed.inject(Router), 'navigate').and.resolveTo(true);
    fixture = TestBed.createComponent(BookFormComponent);
    if (id) {
      fixture.componentRef.setInput('id', id);
    }
    fixture.detectChanges();
    await fixture.whenStable();
  }

  const el = () => fixture.nativeElement as HTMLElement;
  const type = (selector: string, value: string) => {
    const input = el().querySelector<HTMLInputElement>(selector)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  };
  const choose = (selector: string, value: string) => {
    const select = el().querySelector<HTMLSelectElement>(selector)!;
    select.value = value;
    select.dispatchEvent(new Event('change'));
  };
  const submit = async () => {
    el().querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  };

  it('on create, hides status and rating and sends only title and author', async () => {
    await setup();
    expect(el().querySelector('#status')).toBeNull();

    type('#title', 'Refactoring');
    type('#author', 'Martin Fowler');
    await submit();

    expect(booksService.create).toHaveBeenCalledWith({ title: 'Refactoring', author: 'Martin Fowler' });
  });

  it('does not submit an empty form', async () => {
    await setup();
    await submit();
    expect(booksService.create).not.toHaveBeenCalled();
  });

  it('on edit, loads the book into the form', async () => {
    await setup('42');
    expect(booksService.get).toHaveBeenCalledWith('42');
    expect(el().querySelector<HTMLInputElement>('#title')!.value).toBe('Clean Code');
  });

  it('shows the rating only when the status is Read', async () => {
    await setup('42');
    expect(el().querySelector('#rating')).toBeNull();

    choose('#status', 'Read');
    fixture.detectChanges();
    await fixture.whenStable();

    expect(el().querySelector('#rating')).not.toBeNull();
  });

  it('drops the rating when the status is not Read', async () => {
    await setup('42');
    fixture.componentInstance['form'].patchValue({ status: 'Reading', rating: 4 });
    await submit();

    expect(booksService.update).toHaveBeenCalledWith('42', {
      title: 'Clean Code',
      author: 'Robert C. Martin',
      status: 'Reading',
      rating: null,
    });
  });
});
