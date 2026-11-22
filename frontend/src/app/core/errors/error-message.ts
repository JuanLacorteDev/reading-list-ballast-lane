import { HttpErrorResponse } from '@angular/common/http';

/** Shape of an RFC 9457 ProblemDetails body returned by both APIs. */
interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
  errors?: Record<string, string[]>;
}

/**
 * Turns any HTTP error into a sentence the user can read.
 * Validation errors ([ApiController] model binding) come as a map of field -> messages.
 */
export function toErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'Something went wrong. Please try again.';
  }

  if (error.status === 0) {
    return 'Cannot reach the server. Check that the APIs are running.';
  }

  const problem = error.error as ProblemDetails | null;

  if (problem?.errors) {
    const messages = Object.values(problem.errors).flat();
    if (messages.length > 0) {
      return messages.join(' ');
    }
  }

  if (problem?.detail) {
    return problem.detail;
  }

  if (error.status >= 500) {
    return 'The server had a problem. Please try again later.';
  }

  return problem?.title ?? 'Something went wrong. Please try again.';
}
