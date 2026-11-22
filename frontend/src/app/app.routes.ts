import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guards';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'books' },
  {
    path: 'login',
    canActivate: [guestGuard],
    title: 'Sign in | Reading List',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    title: 'Create account | Reading List',
    loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'books',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        title: 'My books | Reading List',
        loadComponent: () => import('./features/books/book-list/book-list.component').then((m) => m.BookListComponent),
      },
      {
        path: 'new',
        title: 'Add book | Reading List',
        loadComponent: () => import('./features/books/book-form/book-form.component').then((m) => m.BookFormComponent),
      },
      {
        path: ':id/edit',
        title: 'Edit book | Reading List',
        loadComponent: () => import('./features/books/book-form/book-form.component').then((m) => m.BookFormComponent),
      },
    ],
  },
  { path: '**', redirectTo: 'books' },
];
