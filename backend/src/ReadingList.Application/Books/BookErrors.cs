using ReadingList.Application.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Application.Books
{
    public static class BookErrors
    {
        public static readonly Error NotFound = Error.NotFound("Book.NotFound", "Book was not found.");
    }
}
