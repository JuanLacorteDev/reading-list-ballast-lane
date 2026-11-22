import { HttpErrorResponse } from '@angular/common/http';
import { toErrorMessage } from './error-message';

describe('toErrorMessage', () => {
  it('uses the ProblemDetails detail', () => {
    const error = new HttpErrorResponse({
      status: 409,
      error: { title: 'Auth.EmailAlreadyInUse', detail: 'Email is already registered.' },
    });
    expect(toErrorMessage(error)).toBe('Email is already registered.');
  });

  it('joins model validation errors', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { errors: { Title: ['The Title field is required.'], Author: ['The Author field is required.'] } },
    });
    expect(toErrorMessage(error)).toBe('The Title field is required. The Author field is required.');
  });

  it('explains when the server cannot be reached', () => {
    expect(toErrorMessage(new HttpErrorResponse({ status: 0 }))).toContain('Cannot reach the server');
  });

  it('hides server internals on 5xx without detail', () => {
    expect(toErrorMessage(new HttpErrorResponse({ status: 500, error: { title: 'NullReferenceException' } })))
      .toContain('The server had a problem');
  });

  it('falls back for non-HTTP errors', () => {
    expect(toErrorMessage(new Error('boom'))).toBe('Something went wrong. Please try again.');
  });
});
