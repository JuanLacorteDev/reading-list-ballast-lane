export const READING_STATUSES = ['WantToRead', 'Reading', 'Read', 'Abandoned'] as const;

export type ReadingStatus = (typeof READING_STATUSES)[number];

export const READING_STATUS_LABELS: Record<ReadingStatus, string> = {
  WantToRead: 'Want to read',
  Reading: 'Reading',
  Read: 'Read',
  Abandoned: 'Abandoned',
};

export interface Book {
  id: string;
  title: string;
  author: string;
  status: ReadingStatus;
  rating: number | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateBookRequest {
  title: string;
  author: string;
}

export interface UpdateBookRequest {
  title: string;
  author: string;
  status: ReadingStatus;
  rating: number | null;
}
